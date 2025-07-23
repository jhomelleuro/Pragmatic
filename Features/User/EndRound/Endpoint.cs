using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using System.Security.Cryptography;
using System.Text;
using Pragmatic.Helpers;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.EndRound
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : EndpointWithoutRequest<object>
    {
        public override void Configure()
        {
            Post("/user/pragmatic/endRound");
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
                    GameId = form["gameId"],
                    RoundId = form["roundId"],
                    BonusCode = form["bonusCode"],
                    Platform = form["platform"],
                    Token = form["token"],
                    RoundDetails = form["roundDetails"],
                    Win = decimal.TryParse(form["win"], out var winVal) ? winVal : null
                };

                // Validate manually
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
                string apiUrl = $"{settings.Value.UserBaseURL}{PragmaticEndpoint.EndRoundUrl.GetPath()}";
                string secretKey = settings.Value.SecretKey;

                logger.Information("Sending request to Pragmatic API (EndRound): {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "providerId", r.ProviderId },
                    { "userId", r.UserId },
                    { "gameId", r.GameId },
                    { "roundId", r.RoundId }
                };

                if (!string.IsNullOrEmpty(r.BonusCode)) formData.Add("bonusCode", r.BonusCode);
                if (!string.IsNullOrEmpty(r.Platform)) formData.Add("platform", r.Platform);
                if (!string.IsNullOrEmpty(r.Token)) formData.Add("token", r.Token);
                if (!string.IsNullOrEmpty(r.RoundDetails)) formData.Add("roundDetails", r.RoundDetails);
                if (r.Win.HasValue) formData.Add("win", r.Win.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));

                // Hash generation
                var sorted = formData.OrderBy(x => x.Key, StringComparer.Ordinal);
                var queryString = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));
                var stringToHash = queryString + secretKey;

                string hash;
                using (var md5 = MD5.Create())
                {
                    var inputBytes = Encoding.UTF8.GetBytes(stringToHash);
                    var hashBytes = md5.ComputeHash(inputBytes);
                    hash = Convert.ToHexString(hashBytes).ToLower();
                }

                logger.Information("Generated hash: {Hash}", hash);
                formData.Add("hash", hash);

                var content = new FormUrlEncodedContent(formData);
                var apiResponse = await httpClient.PostAsync(apiUrl, content, ct);
                var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);

                logger.Information("Pragmatic API Status: {StatusCode}", apiResponse.StatusCode);
                logger.Information("Pragmatic API Response: {Body}", responseBody);

                var parsed = JsonConvert.DeserializeObject<dynamic>(responseBody);

                await SendAsync(parsed, (int)apiResponse.StatusCode, ct);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error processing end round via Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal error occurred while calling Pragmatic API."
                }, 500, ct);
            }
        }
    }
}
