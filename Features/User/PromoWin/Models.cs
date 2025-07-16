using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.PromoWin
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "pragmaticplay";
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public string UserId { get; set; } = "421";
        public string CampaignId { get; set; } = "123456";
        public string CampaignType { get; set; } = "T";
        public decimal Amount { get; set; } = 200.00m;
        public string Currency { get; set; } = "USD";
        public string Reference { get; set; } = "promo-ref-001";

        public string? RoundId { get; set; }
        public string? GameId { get; set; }
        public string? DataType { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.Timestamp).GreaterThan(0);
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.CampaignId).NotEmpty();
            RuleFor(x => x.CampaignType).NotEmpty();
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Currency).NotEmpty();
            RuleFor(x => x.Reference).NotEmpty();
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
