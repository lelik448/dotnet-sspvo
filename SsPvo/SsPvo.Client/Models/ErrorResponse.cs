using SsPvo.Client.Messages.Base;

namespace SsPvo.Client.Models
{
    public class ErrorResponse : IJsonResponse
    {
        public string Error { get; set; }
    }
}
