using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using SsPvo.Client.Extensions;

namespace SsPvo.Client.Messages.Serialization
{
    public class JwtToken
    {
        public JwtToken() { }

        public JwtToken(string source)
        {
            string[] jwtParts = source.Split('.');
            if (jwtParts.Length > 0) Header = jwtParts[0];
            if (jwtParts.Length >= 1) Payload = jwtParts[1];
            if (jwtParts.Length > 1) Signature = jwtParts[2];
        }

        public string Header { get; set; }
        public string Payload { get; set; }
        public string Signature { get; set; }

        public RequestData Decode()
        {
            var msgData = new RequestData();
            try
            {
                if (!string.IsNullOrWhiteSpace(Header)) msgData.JHeader = JObject.Parse(Header.FromBase64String());
                if (!string.IsNullOrWhiteSpace(Payload)) msgData.XPayload = XDocument.Parse(Payload.FromBase64String());
            }
            finally
            {
            }

            return msgData;
        }
    }
}
