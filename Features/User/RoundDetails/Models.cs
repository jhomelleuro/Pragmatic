using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.User.RoundDetails
{
    internal sealed class Request
    {
        public string ProviderId { get; set; } = "pragmaticplay";
        public string UserId { get; set; } = "421";
        public string RoundId { get; set; } = "123451";
        public string SmResult { get; set; } = "1:10;2;9;3;2#3;1;2;1;4#12;12;12;4;1#R#S#VS#222#MV#2,00#MT#2#";
        public string GameCategory { get; set; } = "slot";
        public int BetMultiplier { get; set; } = 50;
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ProviderId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.RoundId).NotEmpty();
            RuleFor(x => x.SmResult).NotEmpty();
            RuleFor(x => x.GameCategory).NotEmpty();
            RuleFor(x => x.BetMultiplier).GreaterThan(0);
        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }
}
