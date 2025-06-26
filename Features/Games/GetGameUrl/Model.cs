using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.Games.GetGameUrl
{
    internal sealed class Request
    {
        public string SecureLogin { get; set; } = "euexs_euroeximsw";
        public string Symbol { get; set; } = "";
        public string Hash { get; set; } = "1a0eac94cf9edcf3be307b45ee55447a";
        public string Language { get; set; } = "EN";
        public string ExternalPlayerId { get; set; } = "";
        public string Currency { get; set; } = "USD";
        public string Platform { get; set; } = "Desktop";
        public string Technology { get; set; } = "H5";
        public string CashierUrl { get; set; } = "https://bet.playtogo.co/wallet";
        public string LobbyUrl { get; set; } = "https://bet.playtogo.co/";
        public string Country { get; set; } = "PH";
        public string RcCloseUrl { get; set; } = "https://bet.playtogo.co/";
        public string PlayMode { get; set; } = "DEMO";
        public string OperatorGameHistory { get; set; } = "https://bet.playtogo.co/transactions";
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {

        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }


}
