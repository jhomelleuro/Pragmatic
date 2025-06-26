using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.Games.HealthCheck
{
    internal sealed class Request
    {

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
