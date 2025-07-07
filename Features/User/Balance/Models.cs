using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.Balance
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "euexs_euroeximsw";
        public string UserId { get; set; } = "421";
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
