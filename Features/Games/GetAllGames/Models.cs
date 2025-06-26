using FluentValidation;
using Pragmatic.Models;

namespace Pragmatic.Pragmatic.Features.Games.GetAllGames
{
    internal sealed class Request
    {
        public string SecureLogin { get; set; } = "euexs_euroeximsw";
        public string Options { get; set; } = "GetFeatures,GetFrbDetails,GetLines,GetDataTypes,GetFcDetails";
        public string Hash { get; set; } = "1a0eac94cf9edcf3be307b45ee55447a"; 
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
