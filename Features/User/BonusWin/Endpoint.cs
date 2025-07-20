using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using Pragmatic.Helpers;
using System.Security.Cryptography;
using System.Text;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.BonusWin
{
    internal sealed class Endpoint(
        Serilog.ILogger logger, IOptions<PragmaticApiSettings> settings) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/user/pragmatic/bonusWin");
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

                // Validate required fields
                var providerId = form["providerId"];
                var userId = form["userId"];
                var amount = form["amount"];
                var reference = form["reference"];
                var bonusCode = form["bonusCode"];
                var timestamp = form["timestamp"];

                if (string.IsNullOrWhiteSpace(providerId) || string.IsNullOrWhiteSpace(userId) ||
                    string.IsNullOrWhiteSpace(amount) || string.IsNullOrWhiteSpace(reference) ||
                    string.IsNullOrWhiteSpace(bonusCode) || string.IsNullOrWhiteSpace(timestamp))
                {
                    await SendAsync(new
                    {
                        error = 400,
                        description = "Missing required fields"
                    }, 400, ct);
                    return;
                }

                var formData = new Dictionary<string, string>
                {
                    { "providerId", providerId },
                    { "userId", userId },
                    { "amount", amount },
                    { "reference", reference },
                    { "bonusCode", bonusCode },
                    { "timestamp", timestamp }
                };

                // Optional fields
                if (!string.IsNullOrWhiteSpace(form["roundId"]))
                    formData["roundId"] = form["roundId"];
                if (!string.IsNullOrWhiteSpace(form["gameId"]))
                    formData["gameId"] = form["gameId"];
                if (!string.IsNullOrWhiteSpace(form["token"]))
                    formData["token"] = form["token"];
                if (!string.IsNullOrWhiteSpace(form["requestId"]))
                    formData["requestId"] = form["requestId"];
                if (!string.IsNullOrWhiteSpace(form["remainAmount"]))
                    formData["remainAmount"] = form["remainAmount"];

                // Hash generation
                var sorted = formData.OrderBy(x => x.Key, StringComparer.Ordinal);
                var queryString = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));
                var stringToHash = queryString + settings.Value.SecretKey;

                using var md5 = MD5.Create();
                var hash = Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).ToLower();
                formData["hash"] = hash;

                logger.Information("Sending BonusWin to Pragmatic API: {FormData}", queryString);
                logger.Information("Generated hash: {Hash}", hash);

                var httpClient = Resolve<HttpClient>();
                var apiUrl = $"{settings.Value.BaseUrl}{PragmaticEndpoint.BonusWinUrl.GetPath()}";

                var content = new FormUrlEncodedContent(formData);
                var apiResponse = await httpClient.PostAsync(apiUrl, content, ct);
                var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);

                logger.Information("API Status: {Status}", apiResponse.StatusCode);
                logger.Information("API Body: {Body}", responseBody);

                // Return raw response
                await SendStringAsync(responseBody, contentType: "application/json", cancellation: ct);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error processing bonus win via Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal server error"
                }, 500, ct);
            }
        }
    }
}
