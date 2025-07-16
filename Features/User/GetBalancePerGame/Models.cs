using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.GetBalancePerGame
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "pragmaticplay";
        public string UserId { get; set; } = "421";
        public string GameIdList { get; set; } = "vs20cd,vs20bl,vs7monkeys";

        public string? Token { get; set; }
        public string? Platform { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.GameIdList).NotEmpty();
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
