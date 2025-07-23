using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pragmatic.Configuration;
using System.Security.Cryptography;
using System.Text;
using Pragmatic.Helpers;
using FluentValidation;
using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Pragmatic.Features.User.RoundDetails
{
    internal sealed class Endpoint(
        Serilog.ILogger logger, IOptions<PragmaticApiSettings> settings) : EndpointWithoutRequest<object>
    {
        public override void Configure()
        {
            Post("/user/pragmatic/roundDetails");
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

                var r = new Request
                {
                    ProviderId = form["providerId"],
                    UserId = form["userId"],
                    RoundId = form["roundId"],
                    SmResult = form["smResult"],
                    GameCategory = form["gameCategory"],
                    BetMultiplier = int.TryParse(form["betMultiplier"], out var mult) ? mult : 0
                };

                var validator = new Validator();
                var validationResult = validator.Validate(r);
                if (!validationResult.IsValid)
                {
                    await SendAsync(new
                    {
                        error = 400,
                        description = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                    }, 400, ct);
                    return;
                }

                var formData = new Dictionary<string, string>
                {
                    { "providerId", r.ProviderId },
                    { "userId", r.UserId },
                    { "roundId", r.RoundId },
                    { "smResult", r.SmResult },
                    { "gameCategory", r.GameCategory },
                    { "betMultiplier", r.BetMultiplier.ToString() }
                };

                var sorted = formData.OrderBy(x => x.Key, StringComparer.Ordinal);
                var queryString = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));
                var stringToHash = queryString + settings.Value.SecretKey;

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
                var httpClient = Resolve<HttpClient>();
                var apiUrl = $"{settings.Value.UserBaseURL}{PragmaticEndpoint.RoundDetailsUrl.GetPath()}";

                var apiResponse = await httpClient.PostAsync(apiUrl, content, ct);
                var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);

                logger.Information("Pragmatic API Status: {StatusCode}", apiResponse.StatusCode);
                logger.Information("Pragmatic API Response: {Body}", responseBody);

                var parsed = JsonConvert.DeserializeObject<dynamic>(responseBody);
                await SendAsync(parsed, (int)apiResponse.StatusCode, ct);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error processing round details via Pragmatic API.");
                await SendAsync(new
                {
                    error = 500,
                    description = "Internal error occurred while calling Pragmatic API."
                }, 500, ct);
            }
        }
    }
}
