using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.Adjustment
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "pragmaticplay";
        public string UserId { get; set; } = "421";
        public string GameId { get; set; } = "rgs1ftest1";
        public string RoundId { get; set; } = "5103268693";
        public decimal Amount { get; set; } = 1.11m;
        public string Reference { get; set; } = "adjustment-ref-001";
        public decimal ValidBetAmount { get; set; } = 1.75m;
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public string? Token { get; set; }
        public string? RoundDetails { get; set; }
        public string? BonusCode { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.GameId).NotEmpty();
            RuleFor(x => x.RoundId).NotEmpty();
            RuleFor(x => x.Amount).NotEqual(0);
            RuleFor(x => x.Reference).NotEmpty();
            RuleFor(x => x.ValidBetAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Timestamp).GreaterThan(0);
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
