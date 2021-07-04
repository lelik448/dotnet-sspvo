using SsPvo.Client.Messages.Base;

namespace SsPvo.Client.Models
{
    public class ResponseTokenResponse : IJsonResponse
    {
        public string ResponseToken { get; set; }
    }
}
