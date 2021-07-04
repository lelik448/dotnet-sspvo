using Microsoft.Extensions.Logging;
using SsPvo.Client.Extensions;
using SsPvo.Client.Messages.Serialization;
using SsPvo.Client.Models;
using System;
using System.Xml.Linq;

namespace SsPvo.Client.Messages
{
    public class ResponseData
    {
        private ErrorResponse _dataErrors;

        public ResponseData(SsPvoMessage message, ResponseMetadata response)
        {
            Message = message;
            Metadata = response;
        }

        public SsPvoMessage Message { get; }
        public ResponseMetadata Metadata { get; }
        public ErrorResponse DataErrors
        {
            get => _dataErrors;
            private set => _dataErrors = value;
        }


        public ValueTuple<TExpected, ResponseData> TryExtractResponse<TExpected, TLogger>(ILogger<TLogger> logger) where TExpected : class
        {
            try
            {
                if (!ExtractData<TExpected>(out var value))
                {
                    if (DataErrors != null)
                    {
                        logger?.LogWarning("API Response: data errors: {@DataErrors}",
                            Utils.SerializeForLog(DataErrors));
                        return (value, this);
                    }

                    logger?.LogError("API Response: unknown data error!", Utils.SerializeForLog(DataErrors));
                    return (value, this);
                }

                logger?.LogInformation("API Response value: {@Value}", Utils.SerializeForLog(value));

                return (value, this);
            }
            catch (Exception e)
            {
                logger?.LogError(e, "API Response Extraction thrown exception: {@ExceptionMessage}!", e.Message);
                throw;
            }
        }

        public bool ExtractData<TExpected>(out TExpected value) where TExpected : class =>
            ExtractData<TExpected, ResponseData>(out value, null);

        public bool ExtractData<TExpected, TLogger>(out TExpected value, ILogger<TLogger> logger = null) where TExpected : class
        {
            _dataErrors = null;
            string data = Metadata?.Content;

            try
            {
                if (data?.IsValidXml() ?? false)
                {
                    value = XDocument.Parse(data) as TExpected;
                    return typeof(TExpected) == typeof(XDocument);
                }

                if (data?.IsValidJson() ?? false)
                {
                    bool isErrorJson = Utils.ValidateJsonSchemaAs<ErrorResponse>(data);
                    DataErrors = isErrorJson ? Utils.DeserializeTo<ErrorResponse>(data) : null;

                    bool asExpected = Utils.ValidateJsonSchemaAs<TExpected>(data);
                    if (asExpected)
                    {
                        value = Utils.DeserializeTo<TExpected>(data) as TExpected;
                        return true;
                    }

                    value = null;
                    return false;
                }

                value = data as TExpected;
                return typeof(TExpected) == typeof(string);
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Error while extracting '{@Content}' to '{@TExpected}'",
                    Metadata?.Content, typeof(TExpected).Name);
                throw;
            }
        }
    }
}
