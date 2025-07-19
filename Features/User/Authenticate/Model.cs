using Newtonsoft.Json;

namespace Pragmatic.Pragmatic.Features.User.Authenticate
{
    public class Model
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("cash")]
        public decimal Cash { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("jurisdiction")]
        public string Jurisdiction { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("error")]
        public int Error { get; set; }
    }
}
