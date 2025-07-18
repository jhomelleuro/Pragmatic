using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using System.Security.Cryptography;
using System.Text;
using Pragmatic.Helpers;
using static Pragmatic.Helpers.PragmaticEndpoints;
using System.Net.Http.Headers;

namespace Pragmatic.Pragmatic.Features.User.Authenticate
{
    internal sealed class Endpoint(
        Serilog.ILogger logger, IOptions<PragmaticApiSettings> settings) : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("/user/pragmatic/authenticate");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken ct)
        {
            var response = new Response();

            try
            {
                var httpClient = Resolve<HttpClient>();

                string apiUrl = $"{settings.Value.UserBaseURL}{PragmaticEndpoint.GetAuthenticateUrl.GetPath()}";
                string secretKey = settings.Value.SecretKey;

                logger.Information("Sending request to Gaming Api API: {Url}", apiUrl);

                var formData = new Dictionary<string, string>();

                string token = r.Token;
                formData.Add("token", token);

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

                formData["hash"] = hash;

                var content = new FormUrlEncodedContent(formData);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var apiResponse = await httpClient.PostAsync(apiUrl, content, ct);
                var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);


                logger.Information("Pragmatic API Status: {StatusCode}", apiResponse.StatusCode, apiResponse.IsSuccessStatusCode);
                logger.Information("Pragmatic API Response: {Body}", responseBody);

                if (apiResponse.IsSuccessStatusCode)
                {
                    dynamic parsed = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    string parsedJson = JsonConvert.SerializeObject(parsed, Formatting.Indented);
                    logger.Information("Parsed JSON:\n{Parsed}", parsedJson);

                    if (parsed?.userId != null)
                    {
                        await SendStringAsync(responseBody, contentType: "application/json", cancellation: ct);
                        return;
                    }
                    else
                    {

                        response = null;
                    }
                }
                else
                {
                    response = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error authenticating user via Pragmatic API.");

                response = null;
            }
   //         finally
  //          {
 //              await SendAsync(response, cancellation: ct);
//            }
        }
    }
}
