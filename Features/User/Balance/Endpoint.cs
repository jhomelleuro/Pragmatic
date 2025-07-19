using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pragmatic.Configuration;
using Pragmatic.Helpers;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.Balance
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : Endpoint<Request>
    {
        public override void Configure()
        {
            Post("/user/pragmatic/balance");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken ct)
        {
            try
            {
                var httpClient = Resolve<HttpClient>();

                string apiUrl = $"{settings.Value.UserBaseURL}{PragmaticEndpoint.GetBalanceUrl.GetPath()}";
                string secretKey = settings.Value.SecretKey;

                logger.Information("Sending request to Pragmatic API (Balance): {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "token", r.Token }
                };

                // Sort and hash
                var sorted = formData.OrderBy(x => x.Key, StringComparer.Ordinal);
                var queryString = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));
                var stringToHash = queryString + secretKey;

                using var md5 = MD5.Create();
                string hash = Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).ToLower();
                formData["hash"] = hash;

                logger.Information("Generated hash: {Hash}", hash);

                var content = new FormUrlEncodedContent(formData);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", r.Token);

                var apiResponse = await httpClient.PostAsync(apiUrl, content, ct);
                var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);

                logger.Information("Pragmatic API Status: {StatusCode}", apiResponse.StatusCode);
                logger.Information("Pragmatic API Response: {Body}", responseBody);

                if (!apiResponse.IsSuccessStatusCode)
                {
                    await SendAsync(new
                    {
                        error = (int)apiResponse.StatusCode,
                        description = "Failed to fetch balance"
                    }, statusCode: (int)apiResponse.StatusCode, cancellation: ct);
                    return;
                }

                var parsed = JsonConvert.DeserializeObject<JObject>(responseBody);

                if (parsed == null)
                {
                    await SendAsync(new
                    {
                        error = 502,
                        description = "Invalid response from Pragmatic API"
                    }, statusCode: 502, cancellation: ct);
                    return;
                }

                int errorCode = parsed["error"]?.ToObject<int>() ?? -1;
                string description = parsed["description"]?.ToString() ?? "Unknown error";

                if (errorCode != 0)
                {
                    await SendAsync(new
                    {
                        error = errorCode,
                        description = description
                    }, statusCode: 400, cancellation: ct);
                    return;
                }

                // ✅ Success
                await SendAsync(parsed, cancellation: ct);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error fetching balance from Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal error occurred while calling Pragmatic API."
                }, statusCode: 500, cancellation: ct);
            }
        }
    }
}
