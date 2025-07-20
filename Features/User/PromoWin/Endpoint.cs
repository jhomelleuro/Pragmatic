using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using System.Security.Cryptography;
using System.Text;
using Pragmatic.Helpers;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.Refund
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("/user/pragmatic/refund");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken ct)
        {
            try
            {
                var httpClient = Resolve<HttpClient>();

                string apiUrl = $"{settings.Value.BaseUrl}{PragmaticEndpoint.RefundUrl.GetPath()}";
                string secretKey = settings.Value.SecretKey;

                logger.Information("Sending request to Pragmatic API (Refund): {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "providerId", r.ProviderId },
                    { "userId", r.UserId },
                    { "reference", r.Reference }
                };

                if (!string.IsNullOrEmpty(r.Platform))
                    formData.Add("platform", r.Platform);

                if (r.Amount.HasValue)
                    formData.Add("amount", r.Amount.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));

                if (!string.IsNullOrEmpty(r.GameId))
                    formData.Add("gameId", r.GameId);

                if (!string.IsNullOrEmpty(r.RoundId))
                    formData.Add("roundId", r.RoundId);

                if (r.Timestamp.HasValue)
                    formData.Add("timestamp", r.Timestamp.Value.ToString());

                if (!string.IsNullOrEmpty(r.RoundDetails))
                    formData.Add("roundDetails", r.RoundDetails);

                if (!string.IsNullOrEmpty(r.BonusCode))
                    formData.Add("bonusCode", r.BonusCode);

                if (!string.IsNullOrEmpty(r.Token))
                    formData.Add("token", r.Token);

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

                var parsed = JsonConvert.DeserializeObject<dynamic>(responseBody);

                // Return the raw parsed response with the same status code
                await SendAsync(parsed, (int)apiResponse.StatusCode, ct);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error processing refund via Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal error occurred while calling Pragmatic API."
                }, 500, ct);
            }
        }
    }
}
