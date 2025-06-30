using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http;
using Pragmatic.Models;
using Pragmatic.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Pragmatic.Pragmatic.Features.Games.GetGameUrl
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("/games/pragmatic/get-game-url");
            AllowAnonymous();
        }


        public override async Task HandleAsync(Request r, CancellationToken ct)
        {
            var response = new Response();

            try
            {
                var httpClient = Resolve<HttpClient>();
                string apiUrl = settings.Value.GetGameUrl;
                string secretKey = settings.Value.SecretKey; 

                logger.Information("Sending request to Pragmatic API: {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "secureLogin", r.SecureLogin },
                    { "symbol", r.Symbol },
                    { "language", r.Language },
                    { "currency", r.Currency },
                    { "platform", r.Platform },
                    { "playMode", r.PlayMode },
                    { "externalPlayerId", r.ExternalPlayerId }
                };

                // Build sorted query string
                var sorted = formData.OrderBy(x => x.Key, StringComparer.Ordinal);
                var queryString = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));

                // Append secret key
                var stringToHash = queryString + secretKey;

                // Compute MD5 hash
                string hash;
                using (var md5 = MD5.Create())
                {
                    var inputBytes = Encoding.UTF8.GetBytes(stringToHash);
                    var hashBytes = md5.ComputeHash(inputBytes);
                    hash = Convert.ToHexString(hashBytes).ToLower();
                }

                logger.Information("Generated hash: {Hash}", hash);

                // Add hash to form data
                formData.Add("hash", hash);

                var content = new FormUrlEncodedContent(formData);
                var apiResponse = await httpClient.PostAsync(apiUrl, content, ct);
                var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);

                logger.Information("Pragmatic API Status: {StatusCode}", apiResponse.StatusCode);
                logger.Information("Pragmatic API Response: {Body}", responseBody);

                if (apiResponse.IsSuccessStatusCode)
                {
                    dynamic parsed = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    if (parsed?.error == "0")
                    {
                        response.IsSuccess = true;
                        response.Message = "Game URL fetched successfully.";
                        response.Result = parsed;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = parsed?.description ?? "Failed to fetch game URL.";
                        response.Result = null;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = $"Failed to fetch game URL. Status code: {apiResponse.StatusCode}";
                    response.Result = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error fetching game URL from Pragmatic API.");
                response.IsSuccess = false;
                response.Message = "Internal error occurred while calling Pragmatic API.";
                response.Result = null;
            }
            finally
            {
                await SendAsync(response, cancellation: ct);
            }
        }
    }
}
