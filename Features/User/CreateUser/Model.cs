using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.CreateUser
{
    internal sealed class Request
    {
        public string SecureLogin { get; set; } = "euexs_euroeximsw";
        public string ExternalPlayerId { get; set; } = "6837d276a3038dda4b1925fb";
        public string Currency { get; set; } = "PHP";

    }

    internal sealed class Validator : FluentValidation.AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.SecureLogin).NotEmpty();
            RuleFor(x => x.Currency).NotEmpty();
            RuleFor(x => x.ExternalPlayerId).NotEmpty();
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
