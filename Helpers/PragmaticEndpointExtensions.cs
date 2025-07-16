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
                PragmaticEndpoint.ResultUrl => "result",
                PragmaticEndpoint.BonusWinUrl => "bonusWin",
                PragmaticEndpoint.JackpotWinUrl => "jackpotWin",
                PragmaticEndpoint.EndRoundUrl => "endRound",
                PragmaticEndpoint.RefundUrl => "refund",
                PragmaticEndpoint.GetBalancePerGameUrl => "getBalancePerGame",
                PragmaticEndpoint.PromoWinUrl => "promoWin",
                PragmaticEndpoint.SessionExpiredUrl => "session/expired", //NOT SURE
                PragmaticEndpoint.AdjustmentUrl => "adjustment",
                PragmaticEndpoint.RoundDetailsUrl => "roundDetails",
                _ => throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, null)
            };
        }
    }
}
