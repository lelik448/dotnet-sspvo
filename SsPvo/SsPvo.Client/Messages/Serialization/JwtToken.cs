using System;
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

        public ValueTuple<JObject, XDocument, string> Decode()
        {
            var res = new
            {
                JHeader = !string.IsNullOrWhiteSpace(Header) ? JObject.Parse(Header.FromBase64String()) : null,
                XPayload = !string.IsNullOrWhiteSpace(Payload) ? XDocument.Parse(Payload.FromBase64String()) : null,
                Signature
            };
            return (res.JHeader, res.XPayload, res.Signature);
        }
    }
}
