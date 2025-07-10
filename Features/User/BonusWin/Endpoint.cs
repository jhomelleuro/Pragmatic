using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using System.Security.Cryptography;
using System.Text;
using Pragmatic.Helpers;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.BonusWin
{
    internal sealed class Endpoint(
        Serilog.ILogger logger, IOptions<PragmaticApiSettings> settings) : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("/user/pragmatic/bonusWin");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken ct)
        {
            var response = new Response();

            try
            {
                var httpClient = Resolve<HttpClient>();

                string apiUrl = $"{settings.Value.BaseUrl}{PragmaticEndpoint.BonusWinUrl.GetPath()}";
                string secretKey = settings.Value.SecretKey;

                logger.Information("Sending request to Pragmatic API (BonusWin): {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "providerId", r.ProviderId },
                    { "userId", r.UserId },
                    { "amount", r.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) },
                    { "reference", r.Reference },
                    { "bonusCode", r.BonusCode },
                    { "timestamp", r.Timestamp.ToString() }
                };

                if (!string.IsNullOrEmpty(r.RoundId))
                    formData.Add("roundId", r.RoundId);

                if (!string.IsNullOrEmpty(r.GameId))
                    formData.Add("gameId", r.GameId);

                if (!string.IsNullOrEmpty(r.Token))
                    formData.Add("token", r.Token);

                if (!string.IsNullOrEmpty(r.RequestId))
                    formData.Add("requestId", r.RequestId);

                if (r.RemainAmount.HasValue)
                    formData.Add("remainAmount", r.RemainAmount.Value.ToString());

                // Sort & generate hash
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

                if (apiResponse.IsSuccessStatusCode)
                {
                    var parsed = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    if (parsed?.error == 0)
                    {
                        response.IsSuccess = true;
                        response.Message = "Bonus win processed successfully.";
                        response.Result = parsed;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = parsed?.description ?? "Failed to process bonus win.";
                        response.Result = parsed;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = $"Failed to process bonus win. Status code: {apiResponse.StatusCode}";
                    response.Result = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error processing bonus win via Pragmatic API.");
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
