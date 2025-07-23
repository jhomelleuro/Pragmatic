using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using Pragmatic.Helpers;
using System.Security.Cryptography;
using System.Text;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.Refund
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/user/pragmatic/refund");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                if (!HttpContext.Request.HasFormContentType)
                {
                    await SendAsync(new
                    {
                        error = 400,
                        description = "Only form-urlencoded content is supported"
                    }, 400, ct);
                    return;
                }

                var form = await HttpContext.Request.ReadFormAsync(ct);

                var r = new Request
                {
                    ProviderId = form["providerId"],
                    UserId = form["userId"],
                    Reference = form["reference"],
                    Platform = form["platform"],
                    Amount = decimal.TryParse(form["amount"], out var amt) ? amt : null,
                    GameId = form["gameId"],
                    RoundId = form["roundId"],
                    Timestamp = long.TryParse(form["timestamp"], out var ts) ? ts : null,
                    RoundDetails = form["roundDetails"],
                    BonusCode = form["bonusCode"],
                    Token = form["token"]
                };

                var validator = new Validator();
                var validationResult = validator.Validate(r);

                if (!validationResult.IsValid)
                {
                    await SendAsync(new
                    {
                        error = 400,
                        description = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                    }, 400, ct);
                    return;
                }

                var httpClient = Resolve<HttpClient>();
                var apiUrl = $"{settings.Value.UserBaseURL}{PragmaticEndpoint.RefundUrl.GetPath()}";
                var secretKey = settings.Value.SecretKey;

                logger.Information("Sending Refund to Pragmatic API: {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "providerId", r.ProviderId },
                    { "userId", r.UserId },
                    { "reference", r.Reference }
                };

                if (!string.IsNullOrEmpty(r.Platform)) formData["platform"] = r.Platform;
                if (r.Amount.HasValue) formData["amount"] = r.Amount.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(r.GameId)) formData["gameId"] = r.GameId;
                if (!string.IsNullOrEmpty(r.RoundId)) formData["roundId"] = r.RoundId;
                if (r.Timestamp.HasValue) formData["timestamp"] = r.Timestamp.Value.ToString();
                if (!string.IsNullOrEmpty(r.RoundDetails)) formData["roundDetails"] = r.RoundDetails;
                if (!string.IsNullOrEmpty(r.BonusCode)) formData["bonusCode"] = r.BonusCode;
                if (!string.IsNullOrEmpty(r.Token)) formData["token"] = r.Token;

                // Hash
                var sorted = formData.OrderBy(x => x.Key, StringComparer.Ordinal);
                var queryString = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));
                var stringToHash = queryString + secretKey;

                using var md5 = MD5.Create();
                var hash = Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).ToLower();
                formData["hash"] = hash;

                logger.Information("Generated hash: {Hash}", hash);

                var content = new FormUrlEncodedContent(formData);
                var apiResponse = await httpClient.PostAsync(apiUrl, content, ct);
                var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);

                logger.Information("API Status: {StatusCode}", apiResponse.StatusCode);
                logger.Information("API Response: {Body}", responseBody);

                var parsed = JsonConvert.DeserializeObject<dynamic>(responseBody);

                if (!apiResponse.IsSuccessStatusCode || parsed?.error != 0)
                {
                    await SendAsync(new
                    {
                        error = parsed?.error ?? (int)apiResponse.StatusCode,
                        description = parsed?.description ?? "Failed to process refund."
                    }, (int)apiResponse.StatusCode, ct);
                    return;
                }

                await SendAsync(parsed, 200, ct);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error processing refund via Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal error occurred while calling Pragmatic API."
                }, 500, ct);
            }
        }
    }
}
