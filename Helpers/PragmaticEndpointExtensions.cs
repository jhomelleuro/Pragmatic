using static Pragmatic.Helpers.PragmaticEndpoints;

namespace Pragmatic.Helpers
{
    public static class PragmaticEndpointExtensions
    {
        public static string GetPath(this PragmaticEndpoint endpoint)
        {
            return endpoint switch
            {
                PragmaticEndpoint.GetGameUrl => "game/url",
                PragmaticEndpoint.GetLobbyGames => "getLobbyGames/",
                PragmaticEndpoint.GetAllGames => "getCasinoGames/",
                PragmaticEndpoint.GetCreateUserUrl => "player/account/create/",
                PragmaticEndpoint.GetAuthenticateUrl => "authenticate",
                PragmaticEndpoint.GetBalanceUrl => "balance",
                PragmaticEndpoint.GetBetUrl => "bet",
                _ => throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, null)
            };
        }
    }
}
