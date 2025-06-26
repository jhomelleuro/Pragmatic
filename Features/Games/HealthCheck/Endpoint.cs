using Newtonsoft.Json;
using Serilog;
using System.Net.Http;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.Games.HealthCheck
{
    internal sealed class Endpoint(Serilog.ILogger logger) : Endpoint<Request, Response>
    {
        private const string PragmaticApiUrl = "https://api.prerelease-env.biz/IntegrationService/v3/http/CasinoGameAPI/health/heartbeatCheck";

        public override void Configure()
        {
            Post("/games/pragmatic/health-check");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken ct)
        {
            var response = new Response();

            try
            {
                var httpClient = Resolve<HttpClient>();

                logger.Information("Sending request to Pragmatic API: {Url}", PragmaticApiUrl);

                var apiResponse = await httpClient.GetAsync(PragmaticApiUrl, ct);
                var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);

                logger.Information("Pragmatic API Status: {StatusCode}", apiResponse.StatusCode);
                logger.Information("Pragmatic API Response: {Body}", responseBody);

                if (apiResponse.IsSuccessStatusCode)
                {
                    dynamic parsed = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    if (parsed?.error == "0")
                    {

                        var withImageData =
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
