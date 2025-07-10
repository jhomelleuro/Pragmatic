using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.Result
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "pragmaticplay";
        public string UserId { get; set; } = "421";
        public string GameId { get; set; } = "vs50aladdin";
        public string RoundId { get; set; } = "5103268693";
        public decimal Amount { get; set; } = 10.0m;
        public string Reference { get; set; } = "unique-ref-002";
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public string RoundDetails { get; set; } = "spin";

        public string? BonusCode { get; set; }
        public string? Platform { get; set; }
        public string? Token { get; set; }

        public decimal? PromoWinAmount { get; set; }
        public string? PromoWinReference { get; set; }
        public string? PromoCampaignID { get; set; }
        public string? PromoCampaignType { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.GameId).NotEmpty();
            RuleFor(x => x.RoundId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Reference).NotEmpty();
            RuleFor(x => x.Timestamp).GreaterThan(0);
            RuleFor(x => x.RoundDetails).NotEmpty();

            // If promoWinAmount is present, others must be present
            When(x => x.PromoWinAmount.HasValue, () =>
            {
                RuleFor(x => x.PromoWinReference).NotEmpty();
                RuleFor(x => x.PromoCampaignID).NotEmpty();
                RuleFor(x => x.PromoCampaignType).NotEmpty();
            });
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
