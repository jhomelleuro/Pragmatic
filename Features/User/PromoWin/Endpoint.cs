using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using Pragmatic.Helpers;
using FluentValidation;
using System.Security.Cryptography;
using System.Text;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.PromoWin
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : EndpointWithoutRequest<object>
    {
        public override void Configure()
        {
            Post("/user/pragmatic/promoWin");
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
                    Timestamp = long.TryParse(form["timestamp"], out var ts) ? ts : 0,
                    UserId = form["userId"],
                    CampaignId = form["campaignId"],
                    CampaignType = form["campaignType"],
                    Amount = decimal.TryParse(form["amount"], out var amt) ? amt : 0,
                    Currency = form["currency"],
                    Reference = form["reference"],
                    RoundId = form["roundId"],
                    GameId = form["gameId"],
                    DataType = form["dataType"]
                };

                // Manual validation
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

                var formData = new Dictionary<string, string>
                {
                    { "providerId", r.ProviderId },
                    { "timestamp", r.Timestamp.ToString() },
                    { "userId", r.UserId },
                    { "campaignId", r.CampaignId },
                    { "campaignType", r.CampaignType },
                    { "amount", r.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) },
                    { "currency", r.Currency },
                    { "reference", r.Reference }
                };

                if (!string.IsNullOrWhiteSpace(r.RoundId)) formData["roundId"] = r.RoundId;
                if (!string.IsNullOrWhiteSpace(r.GameId)) formData["gameId"] = r.GameId;
                if (!string.IsNullOrWhiteSpace(r.DataType)) formData["dataType"] = r.DataType;

                // Sort & generate hash
                var sorted = formData.OrderBy(x => x.Key, StringComparer.Ordinal);
                var queryString = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));
                var stringToHash = queryString + settings.Value.SecretKey;

                string hash;
                using (var md5 = MD5.Create())
                {
                    var inputBytes = Encoding.UTF8.GetBytes(stringToHash);
                    var hashBytes = md5.ComputeHash(inputBytes);
                    hash = Convert.ToHexString(hashBytes).ToLower();
                }

                formData["hash"] = hash;

                var httpClient = Resolve<HttpClient>();
                string apiUrl = $"{settings.Value.UserBaseURL}{PragmaticEndpoint.RefundUrl.GetPath()}";

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
                logger.Error(ex, "Error processing promoWin via Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal server error"
                }, 500, ct);
            }
        }
    }
}
