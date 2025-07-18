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
                PragmaticEndpoint.GetAuthenticateUrl => "v1/pragmatic-play/authenticate",
                PragmaticEndpoint.GetBalanceUrl => "v1/pragmatic-play/balance",
                PragmaticEndpoint.GetBetUrl => "v1/pragmatic-play/bet",
                PragmaticEndpoint.ResultUrl => "v1/pragmatic-play/result",
                PragmaticEndpoint.BonusWinUrl => "v1/pragmatic-play/bonusWin",
                PragmaticEndpoint.JackpotWinUrl => "v1/pragmatic-play/jackpotWin",
                PragmaticEndpoint.EndRoundUrl => "v1/pragmatic-play/endRound",
                PragmaticEndpoint.RefundUrl => "v1/pragmatic-play/refund",
                PragmaticEndpoint.GetBalancePerGameUrl => "v1/pragmatic-play/getBalancePerGame",
                PragmaticEndpoint.PromoWinUrl => "v1/pragmatic-play/promoWin",
                PragmaticEndpoint.SessionExpiredUrl => "session/expired", //NOT SURE
                PragmaticEndpoint.AdjustmentUrl => "v1/pragmatic-play/adjustment",
                PragmaticEndpoint.RoundDetailsUrl => "roundDetails",
                _ => throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, null)
            };
        }
    }
}
