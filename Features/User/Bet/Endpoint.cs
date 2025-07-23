using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using Pragmatic.Helpers;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.Bet
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/user/pragmatic/bet");
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
                var formDataAsString = string.Join(", ", form.Select(kv => $"{kv.Key}={kv.Value}"));
                logger.Information("Received form data: {FormData}", formDataAsString);
                var token = form["Token"].ToString();
                var amountStr = form["Amount"].ToString();
                var reference = form["Reference"].ToString();
                var roundDetails = form["roundDetails"].ToString();
                var gameId = form["gameId"].ToString();
                var roundId = form["roundId"].ToString();

                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(amountStr) || string.IsNullOrEmpty(reference))
                {
                    await SendAsync(new
                    {
                        error = 400,
                        description = "Token and Amount are required"
                    }, 400, ct);
                    return;
                }

                if (!decimal.TryParse(amountStr, out var amount))
                {
                    await SendAsync(new
                    {
                        error = 400,
                        description = "Invalid amount format"
                    }, 400, ct);
                    return;
                }

                var httpClient = Resolve<HttpClient>();
                string apiUrl = $"{settings.Value.UserBaseURL}{PragmaticEndpoint.GetBetUrl.GetPath()}";
                string secretKey = settings.Value.SecretKey;

                logger.Information("Sending request to Pragmatic API (Bet): {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "token", token },
                    { "amount", amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) },
                    { "reference", reference },
                    { "roundDetails", roundDetails },
                    { "gameId", gameId },
                    { "roundId", roundId }
                };

                var sorted = formData.OrderBy(x => x.Key, StringComparer.Ordinal);
                string queryString = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));
                string stringToHash = queryString + secretKey;

                using var md5 = MD5.Create();
                string hash = Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).ToLower();
                formData["hash"] = hash;

                logger.Information("Generated hash: {Hash}", hash);

                var content = new FormUrlEncodedContent(formData);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiResponse = await httpClient.PostAsync(apiUrl, content, ct);
                var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);

                logger.Information("Pragmatic API Status: {StatusCode}", apiResponse.StatusCode);
                logger.Information("Pragmatic API Response: {Body}", responseBody);

                if (!apiResponse.IsSuccessStatusCode)
                {
                    await SendAsync(new
                    {
                        error = (int)apiResponse.StatusCode,
                        description = "Failed to place bet"
                    }, (int)apiResponse.StatusCode, ct);
                    return;
                }

                var parsed = JsonConvert.DeserializeObject<dynamic>(responseBody);

                if (parsed == null)
                {
                    await SendAsync(new
                    {
                        error = 502,
                        description = "Invalid response from Pragmatic API"
                    }, 502, ct);
                    return;
                }

                if (parsed.transactionId == null)
                {
                    await SendAsync(new
                    {
                        error = 400,
                        description = parsed.description ?? "Failed to place bet",
                        result = parsed
                    }, 400, ct);
                    return;
                }

                await SendAsync(parsed);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error placing bet via Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal error occurred while calling Pragmatic API."
                }, 500, ct);
            }
        }
    }
}
