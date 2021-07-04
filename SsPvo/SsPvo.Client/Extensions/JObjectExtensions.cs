using Newtonsoft.Json.Linq;
using System;
using SsPvo.Client.Messages.Serialization;

namespace SsPvo.Client.Extensions
{
    public static class JObjectExtensions
    {
        public static JwtToken GetToken(this JObject jo, string jsonPropertyName)
        {
            var jt = jo.GetValue(jsonPropertyName, StringComparison.CurrentCultureIgnoreCase);
            if (jt == null) return null;

            string jwt = jt.Value<string>();
            return new JwtToken(jwt);
        }
    }
}
