using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;
using RestSharp;
using SsPvo.Client.Extensions;
using SsPvo.Client.Messages.Exceptions;

namespace SsPvo.Client.Messages.Serialization
{
    public static class Utils
    {
        static Utils()
        {
            SerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            SerializerSettings.Converters.Add(new JObjectNamingStrategyConverter(new CamelCaseNamingStrategy()));

            LoggingSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            SerializerSettings.Converters.Add(new JObjectNamingStrategyConverter(new CamelCaseNamingStrategy()));

            IgnoreSerializationErrorsSettings = new JsonSerializerSettings
            {
                Error = delegate(object sender, ErrorEventArgs args) { args.ErrorContext.Handled = true; }
            };

            JSchemas = new Dictionary<Type, JSchema>();
            var generator = new JSchemaGenerator
            {
                ContractResolver = SerializerSettings.ContractResolver
            };

            Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.FullName?.StartsWith("SsPvo.Client.Models.") ?? false)
                .ToList()
                .ForEach(type => ((Dictionary<Type, JSchema>)JSchemas).Add(type, generator.Generate(type)));
        }

        public static IReadOnlyDictionary<Type, JSchema> JSchemas { get; }
        public static JsonSerializerSettings SerializerSettings { get; set; }
        public static JsonSerializerSettings LoggingSerializerSettings { get; set; }
        private static JsonSerializerSettings IgnoreSerializationErrorsSettings { get; }

        internal static JObject GetSignedObject(ICsp cryptoService, RequestData msgJwt, JsonSerializerSettings serializerSettings)
        {
            var joHeader = msgJwt.JHeader;
            string jsonHeader = JsonConvert.SerializeObject(joHeader, serializerSettings);
            string b64Header = jsonHeader.ToBase64String();

            var xmlPayload = msgJwt.XPayload;
            string b64Payload = xmlPayload?.ToString().ToBase64String();

            string stringToSign = $"{b64Header}.{b64Payload}";

            try
            {
                byte[] signed = cryptoService.SignData(System.Text.Encoding.UTF8.GetBytes(stringToSign));

                string signature = Convert.ToBase64String(signed);

                return new JObject
                {
                    { "token", $"{b64Header}.{b64Payload}.{signature}" }
                };
            }
            catch (Exception e)
            {
                throw new SsPvoDataSigningException(e.Message, e);
            }
        }


        public static string SerializeForLog(object obj, JsonSerializerSettings overrideSettings = null) =>
            GetSerialized(obj, overrideSettings ?? LoggingSerializerSettings);

        public static string GetSerialized(object obj, JsonSerializerSettings overrideSettings = null)
        {
            if (obj is XDocument xdoc) return xdoc.ToString(SaveOptions.DisableFormatting);
            return JsonConvert.SerializeObject(obj, overrideSettings ?? SerializerSettings);
        }

        public static bool ValidateJsonSchemaAs<T>(string source)
        {
            if (!(source?.IsValidJson() ?? false)) return false;
            if (!JSchemas.ContainsKey(typeof(T)))
            {
                var generator = new JSchemaGenerator();
                ((Dictionary<Type, JSchema>)JSchemas).Add(typeof(T), generator.Generate(typeof(T)));
            }

            IList<string> messages;
            var jo = JObject.Parse(source);
            bool isValid = jo.IsValid(JSchemas[typeof(T)], out messages);
            return isValid;
        }

        public static T DeserializeTo<T>(string source) where T : class =>
            JsonConvert.DeserializeObject<T>(source, SerializerSettings);

        public static void LogRequest<TServiceType>(Guid messageGuid, IRestClient restClient, ILogger<TServiceType> logger, IRestRequest request, IRestResponse response)
        {
            if (restClient == null) throw new ArgumentNullException(nameof(restClient));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var requestToLog = new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = restClient.BuildUri(request),
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };

            logger.LogTrace("Request '{@MessageGuid}' completed. RequestData: {@RequestToLog}, Response: {@ResponseToLog}",
                messageGuid, Utils.SerializeForLog(requestToLog), Utils.SerializeForLog(responseToLog));
        }
    }
}
