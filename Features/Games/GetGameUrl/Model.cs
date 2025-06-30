using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.Games.GetGameUrl
{
    internal sealed class Request
    {
        public string SecureLogin { get; set; } = "euexs_euroeximsw";
        public string Symbol { get; set; } = "vs20olympgold";
        public string Language { get; set; } = "en";
        public string Currency { get; set; } = "PHP";
        public string Platform { get; set; } = "WEB";
        public string PlayMode { get; set; } = "DEMO";
        public string ExternalPlayerId { get; set; } = "6837d276a3038dda4b1925fb";
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
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }

}
