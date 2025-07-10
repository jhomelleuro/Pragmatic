using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.JackpotWin
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "pragmaticplay";
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public string UserId { get; set; } = "421";
        public string GameId { get; set; } = "vs30catz_jp";
        public string RoundId { get; set; } = "5109164607";
        public string JackpotId { get; set; } = "568";
        public decimal Amount { get; set; } = 55.0m;
        public string Reference { get; set; } = "unique-ref-004";

        public string? JackpotDetails { get; set; }
        public string? Platform { get; set; }
        public string? Token { get; set; }
        public string? BalanceBeforeWin { get; set; }
        public string? BalanceAfterWin { get; set; }
        public string? InstanceId { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.Timestamp).GreaterThan(0);
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.GameId).NotEmpty();
            RuleFor(x => x.RoundId).NotEmpty();
            RuleFor(x => x.JackpotId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Reference).NotEmpty();
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
