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
                PragmaticEndpoint.GetAuthenticateUrl => "v1/pragmatic-user/authenticate",
                PragmaticEndpoint.GetBalanceUrl => "v1/pragmatic-user/balance",
                PragmaticEndpoint.GetBetUrl => "v1/pragmatic-user/bet",
                PragmaticEndpoint.ResultUrl => "v1/pragmatic-user/result",
                PragmaticEndpoint.BonusWinUrl => "v1/pragmatic-user/bonusWin",
                PragmaticEndpoint.JackpotWinUrl => "v1/pragmatic-user/jackpotWin",
                PragmaticEndpoint.EndRoundUrl => "v1/pragmatic-user/endRound",
                PragmaticEndpoint.RefundUrl => "v1/pragmatic-user/refund",
                PragmaticEndpoint.GetBalancePerGameUrl => "v1/pragmatic-user/getBalancePerGame",
                PragmaticEndpoint.PromoWinUrl => "v1/pragmatic-user/promoWin",
                PragmaticEndpoint.SessionExpiredUrl => "session/expired", //NOT SURE
                PragmaticEndpoint.AdjustmentUrl => "v1/pragmatic-user/adjustment",
                PragmaticEndpoint.RoundDetailsUrl => "roundDetails",
                _ => throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, null)
            };
        }
    }
}
