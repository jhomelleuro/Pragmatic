using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using Pragmatic.Helpers;
using FluentValidation;
using System.Security.Cryptography;
using System.Text;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.Result
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : EndpointWithoutRequest<object>
    {
        public override void Configure()
        {
            Post("/user/pragmatic/result");
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
                    Amount = decimal.TryParse(form["amount"], out var amt) ? amt : 0,
                    Reference = form["reference"],
                    Timestamp = long.TryParse(form["timestamp"], out var ts) ? ts : 0,
                    RoundDetails = form["roundDetails"],
                    BonusCode = form["bonusCode"],
                    Platform = form["platform"],
                    Token = form["token"],
                    PromoWinAmount = decimal.TryParse(form["promoWinAmount"], out var promoAmt) ? promoAmt : null,
                    PromoWinReference = form["promoWinReference"],
                    PromoCampaignID = form["promoCampaignID"],
                    PromoCampaignType = form["promoCampaignType"]
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
                    { "userId", r.UserId },
                    { "gameId", r.GameId },
                    { "roundId", r.RoundId },
                    { "amount", r.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) },
                    { "reference", r.Reference },
                    { "timestamp", r.Timestamp.ToString() },
                    { "roundDetails", r.RoundDetails }
                };

                if (!string.IsNullOrWhiteSpace(r.BonusCode)) formData["bonusCode"] = r.BonusCode;
                if (!string.IsNullOrWhiteSpace(r.Platform)) formData["platform"] = r.Platform;
                if (!string.IsNullOrWhiteSpace(r.Token)) formData["token"] = r.Token;

                if (r.PromoWinAmount.HasValue &&
                    !string.IsNullOrWhiteSpace(r.PromoWinReference) &&
                    !string.IsNullOrWhiteSpace(r.PromoCampaignID) &&
                    !string.IsNullOrWhiteSpace(r.PromoCampaignType))
                {
                    formData["promoWinAmount"] = r.PromoWinAmount.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    formData["promoWinReference"] = r.PromoWinReference;
                    formData["promoCampaignID"] = r.PromoCampaignID;
                    formData["promoCampaignType"] = r.PromoCampaignType;
                }

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
                string apiUrl = $"{settings.Value.UserBaseURL}{PragmaticEndpoint.ResultUrl.GetPath()}";

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
                logger.Error(ex, "Error processing result via Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal server error"
                }, 500, ct);
            }
        }
    }
}
