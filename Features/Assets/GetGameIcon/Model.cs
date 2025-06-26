using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.Assets.GetGameIcon
{
    internal sealed class Request
    {
        public string GameId { get; set; } = "vs20olympgate";
        public string size { get; set; }
    }

    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {

        }
    }

    internal sealed class Response : ResponseModel<object?>
    {
    }


}
