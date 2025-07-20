using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using System.Security.Cryptography;
using System.Text;
using Pragmatic.Helpers;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.Adjustment
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/user/pragmatic/adjustment");
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

                // Extract & validate required fields
                string providerId = form["providerId"];
                string userId = form["userId"];
                string gameId = form["gameId"];
                string roundId = form["roundId"];
                string amount = form["amount"];
                string reference = form["reference"];
                string validBetAmount = form["validBetAmount"];
                string timestamp = form["timestamp"];

                if (string.IsNullOrWhiteSpace(providerId) ||
                    string.IsNullOrWhiteSpace(userId) ||
                    string.IsNullOrWhiteSpace(gameId) ||
                    string.IsNullOrWhiteSpace(roundId) ||
                    string.IsNullOrWhiteSpace(amount) ||
                    string.IsNullOrWhiteSpace(reference) ||
                    string.IsNullOrWhiteSpace(validBetAmount) ||
                    string.IsNullOrWhiteSpace(timestamp))
                {
                    await SendAsync(new
                    {
                        error = 400,
                        description = "Missing required fields"
                    }, 400, ct);
                    return;
                }

                // Additional type/format checks
                if (!decimal.TryParse(amount, out var amountValue) || amountValue == 0 ||
                    !decimal.TryParse(validBetAmount, out _) ||
                    !long.TryParse(timestamp, out var timestampValue) || timestampValue <= 0)
                {
                    await SendAsync(new
                    {
                        error = 400,
                        description = "Invalid value in amount, validBetAmount, or timestamp"
                    }, 400, ct);
                    return;
                }


                var httpClient = Resolve<HttpClient>();
                string apiUrl = $"{settings.Value.BaseUrl}{PragmaticEndpoint.AdjustmentUrl.GetPath()}";
                string secretKey = settings.Value.SecretKey;

                logger.Information("Sending request to Pragmatic API (Adjustment): {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "providerId", providerId },
                    { "userId", userId },
                    { "gameId", gameId },
                    { "roundId", roundId },
                    { "amount", amount },
                    { "reference", reference },
                    { "validBetAmount", validBetAmount },
                    { "timestamp", timestamp }
                };

                // Optional values
                if (!string.IsNullOrEmpty(form["token"]))
                    formData["token"] = form["token"];
                if (!string.IsNullOrEmpty(form["roundDetails"]))
                    formData["roundDetails"] = form["roundDetails"];
                if (!string.IsNullOrEmpty(form["bonusCode"]))
                    formData["bonusCode"] = form["bonusCode"];

                // Sort & generate hash
                var sorted = formData.OrderBy(x => x.Key, StringComparer.Ordinal);
                string queryString = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));
                string stringToHash = queryString + secretKey;

                using var md5 = MD5.Create();
                string hash = Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).ToLower();
                logger.Information("Generated hash: {Hash}", hash);

                formData["hash"] = hash;

                var content = new FormUrlEncodedContent(formData);
                var apiResponse = await httpClient.PostAsync(apiUrl, content, ct);
                var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);

                logger.Information("Pragmatic API Status: {StatusCode}", apiResponse.StatusCode);
                logger.Information("Pragmatic API Response: {Body}", responseBody);

                // Always return raw body as JSON
                await SendStringAsync(responseBody, contentType: "application/json", cancellation: ct);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error processing adjustment via Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal error occurred while calling Pragmatic API."
                }, 500, ct);
            }
        }
    }
}
