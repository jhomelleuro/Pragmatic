using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.BonusWin
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "pragmaticplay";
        public string UserId { get; set; } = "421";
        public decimal Amount { get; set; } = 1.0m;
        public string Reference { get; set; } = "unique-ref-003";
        public string BonusCode { get; set; } = "test_pp_frb1";
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public string? RoundId { get; set; }
        public string? GameId { get; set; }
        public string? Token { get; set; }
        public string? RequestId { get; set; }
        public int? RemainAmount { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Reference).NotEmpty();
            RuleFor(x => x.BonusCode).NotEmpty();
            RuleFor(x => x.Timestamp).GreaterThan(0);
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
