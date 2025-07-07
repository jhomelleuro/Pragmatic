using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.Bet
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "euexs_euroeximsw";
        public string UserId { get; set; } = "421";
        public string GameId { get; set; } = "vs50aladdin";
        public string RoundId { get; set; } = "5103188801";
        public decimal Amount { get; set; } = 100.0m;
        public string Reference { get; set; } = "unique-ref-001";
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public string RoundDetails { get; set; } = "spin";
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.GameId).NotEmpty();
            RuleFor(x => x.RoundId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Reference).NotEmpty();
            RuleFor(x => x.Timestamp).GreaterThan(0);
            RuleFor(x => x.RoundDetails).NotEmpty();
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
