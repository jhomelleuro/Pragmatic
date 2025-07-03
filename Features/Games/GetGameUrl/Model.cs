using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.Games.GetGameUrl
{
    internal sealed class Request
    {
        public string SecureLogin { get; set; } = "euexs_euroeximsw";
        public string Symbol { get; set; } = "vs20olympgold";
        public string StyleName { get; set; } = "euexs_euroeximsw";
        public string Language { get; set; } = "en";
        public string Currency { get; set; } = "PHP";
        public string Platform { get; set; } = "WEB";
        public string PlayMode { get; set; } = "DEMO";
        public string ExternalPlayerId { get; set; } = "7190864";
        public string LobbyUrl { get; set; } = "http://127.0.0.1:2222/";
        public string CashierUrl { get; set; } = "http://127.0.0.1:2222/wallet";
        public string RcHistoryUrl { get; set; } = "http://127.0.0.1:2222/transaction";
        public string Token { get; set; } = "7190864";
    }

    internal sealed class Validator : FluentValidation.AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.SecureLogin).NotEmpty();
            RuleFor(x => x.Symbol).NotEmpty();
            RuleFor(x => x.Language).NotEmpty();
            RuleFor(x => x.Currency).NotEmpty();
            RuleFor(x => x.Platform).NotEmpty();
            RuleFor(x => x.PlayMode).NotEmpty();
            RuleFor(x => x.ExternalPlayerId).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }

}
