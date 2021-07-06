using Newtonsoft.Json;
using SsPvo.Client.Messages.Base;

namespace SsPvo.Client.Models
{
    public class ResponseTokenResponse : IJsonResponse
    {
        [JsonProperty("responseToken")]
        public string ResponseToken { get; set; }
    }
}
