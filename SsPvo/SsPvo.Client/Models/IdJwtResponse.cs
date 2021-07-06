using Newtonsoft.Json;
using SsPvo.Client.Messages.Base;

namespace SsPvo.Client.Models
{
    public class IdJwtResponse : IJsonResponse
    {
        [JsonProperty("idJwt")]
        public string IdJwt { get; set; }
    }
}
