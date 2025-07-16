using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.SessionExpired
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "pragmaticplay";
        public string SessionId { get; set; } = "6fd2d6f3bb8f4c5a9fadf15d81206af2";
        public string PlayerId { get; set; } = "421";
        public string? Token { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.SessionId).NotEmpty();
            RuleFor(x => x.PlayerId).NotEmpty();
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
