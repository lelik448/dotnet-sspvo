using Newtonsoft.Json;
using SsPvo.Client.Messages.Base;

namespace SsPvo.Client.Models
{
    public class ErrorResponse : IJsonResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
