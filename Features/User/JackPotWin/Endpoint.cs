using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using System.Security.Cryptography;
using System.Text;
using Pragmatic.Helpers;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.JackpotWin
{
    internal sealed class Endpoint(
        Serilog.ILogger logger, IOptions<PragmaticApiSettings> settings) : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("/user/pragmatic/jackpotWin");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken ct)
        {
            var response = new Response();

            try
            {
                var httpClient = Resolve<HttpClient>();

                string apiUrl = $"{settings.Value.BaseUrl}{PragmaticEndpoint.JackpotWinUrl.GetPath()}";
                string secretKey = settings.Value.SecretKey;

                logger.Information("Sending request to Pragmatic API (JackpotWin): {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "providerId", r.ProviderId },
                    { "timestamp", r.Timestamp.ToString() },
                    { "userId", r.UserId },
                    { "gameId", r.GameId },
                    { "roundId", r.RoundId },
                    { "jackpotId", r.JackpotId },
                    { "amount", r.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) },
                    { "reference", r.Reference }
                };

                if (!string.IsNullOrEmpty(r.JackpotDetails))
                    formData.Add("jackpotDetails", r.JackpotDetails);

                if (!string.IsNullOrEmpty(r.Platform))
                    formData.Add("platform", r.Platform);

                if (!string.IsNullOrEmpty(r.Token))
                    formData.Add("token", r.Token);

                if (!string.IsNullOrEmpty(r.BalanceBeforeWin))
                    formData.Add("balanceBeforeWin", r.BalanceBeforeWin);

                if (!string.IsNullOrEmpty(r.BalanceAfterWin))
                    formData.Add("balanceAfterWin", r.BalanceAfterWin);

                if (!string.IsNullOrEmpty(r.InstanceId))
                    formData.Add("instanceId", r.InstanceId);

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
                        response.Message = "Jackpot win processed successfully.";
                        response.Result = parsed;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = parsed?.description ?? "Failed to process jackpot win.";
                        response.Result = parsed;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = $"Failed to process jackpot win. Status code: {apiResponse.StatusCode}";
                    response.Result = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error processing jackpot win via Pragmatic API.");
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
