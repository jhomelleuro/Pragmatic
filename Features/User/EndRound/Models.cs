using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.EndRound
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "pragmaticplay";
        public string UserId { get; set; } = "421";
        public string GameId { get; set; } = "vs50hercules";
        public string RoundId { get; set; } = "5103579948";

        public string? BonusCode { get; set; }
        public string? Platform { get; set; }
        public string? Token { get; set; }
        public string? RoundDetails { get; set; }
        public decimal? Win { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.GameId).NotEmpty();
            RuleFor(x => x.RoundId).NotEmpty();
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
