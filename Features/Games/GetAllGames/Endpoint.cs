using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http;
using Pragmatic.Models;
using Pragmatic.Configuration;

namespace Pragmatic.Pragmatic.Features.Games.GetAllGames
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticApiSettings> settings) : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("/games/pragmatic/get-all");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken ct)
        {
            var response = new Response();

            try
            {
                var httpClient = Resolve<HttpClient>();
                string apiUrl = settings.Value.GetGameUrl;

                logger.Information("Sending request to Pragmatic API: {Url}", apiUrl);

                var formData = new Dictionary<string, string>
                {
                    { "secureLogin", r.SecureLogin },
                    { "options", r.Options },
                    { "hash", r.Hash }
                };

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
                        response.Message = "Games fetched successfully.";
                        response.Result = parsed;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = parsed?.description ?? "Failed to fetch games.";
                        response.Result = null;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = $"Failed to fetch games. Status code: {apiResponse.StatusCode}";
                    response.Result = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error fetching games from Pragmatic API.");
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
