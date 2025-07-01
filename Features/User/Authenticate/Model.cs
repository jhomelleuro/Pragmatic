using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.Authenticate
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "euexs_euroeximsw";
        public string Token { get; set; } = "7189058";

    }

    internal sealed class Validator : FluentValidation.AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();

        }
    }
    internal sealed class Response : ResponseModel<object?>
    {
    }
}
