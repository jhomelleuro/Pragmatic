using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.Refund
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "pragmaticplay";
        public string UserId { get; set; } = "421";
        public string Reference { get; set; } = "original-bet-ref-001";

        public string? Platform { get; set; }
        public decimal? Amount { get; set; }
        public string? GameId { get; set; }
        public string? RoundId { get; set; }
        public long? Timestamp { get; set; }
        public string? RoundDetails { get; set; }
        public string? BonusCode { get; set; }
        public string? Token { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Reference).NotEmpty();
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
