using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using Pragmatic.Models;
using Serilog;
using System.Net.Http;
using System.Text;

namespace Pragmatic.Pragmatic.Features.Assets.GetGameIcon
{
    internal sealed class Endpoint(
        Serilog.ILogger logger,
        IOptions<PragmaticAssetSettings> assetSettings) : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("/assets/pragmatic/get-game-icon");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request r, CancellationToken ct)
        {
            var response = new Response();

            try
            {
                var httpClient = Resolve<HttpClient>();
                
                string baseUrl = assetSettings.Value.GameIconBaseUrl.TrimEnd('/');
                string imageUrl = $"{baseUrl}/{r.GameId}/{r.GameId}_260x350_NB.png";

                var content = new StringContent("{}", Encoding.UTF8, "application/json");

                var apiResponse = await httpClient.PostAsync(imageUrl, content, ct);
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
