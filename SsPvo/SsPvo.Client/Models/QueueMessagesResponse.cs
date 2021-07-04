using Newtonsoft.Json;
using SsPvo.Client.Messages.Base;

namespace SsPvo.Client.Models
{
    public class QueueMessagesResponse : IJsonResponse
    {
        public int Messages { get; set; }

        [JsonProperty(Required = Required.Default)]
        public int[] IdJwts { get; set; }
    }
}
