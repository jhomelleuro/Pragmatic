using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using Pragmatic.Helpers;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.Authenticate
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/user/pragmatic/authenticate");
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
                var token = form["Token"].ToString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    await SendAsync(new
                    {
                        error = 400,
                        description = "Token is required"
                    }, 400, ct);
                    return;
                }

                var httpClient = Resolve<HttpClient>();
                string apiUrl = $"{settings.Value.UserBaseURL}{PragmaticEndpoint.GetAuthenticateUrl.GetPath()}";
                string secretKey = settings.Value.SecretKey;

                logger.Information("Sending request to Gaming API: {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "token", token }
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
                        description = "Failed to authenticate user"
                    }, (int)apiResponse.StatusCode, ct);
                    return;
                }

                var parsed = JsonConvert.DeserializeObject<Model>(responseBody);

                if (parsed == null)
                {
                    await SendAsync(new
                    {
                        error = 502,
                        description = "Invalid response from Pragmatic API"
                    }, 502, ct);
                    return;
                }

                if (parsed.Error != 0)
                {
                    await SendAsync(new
                    {
                        error = parsed.Error,
                        description = parsed.Description ?? "Authentication failed"
                    }, 400, ct);
                    return;
                }

                await SendAsync(parsed);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error authenticating user via Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal error occurred while calling Pragmatic API."
                }, 500, ct);
            }
        }
    }
}
