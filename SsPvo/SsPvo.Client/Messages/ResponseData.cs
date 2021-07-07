using Microsoft.Extensions.Logging;
using SsPvo.Client.Extensions;
using SsPvo.Client.Messages.Serialization;
using SsPvo.Client.Models;
using System;
using System.Net.Mime;
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
                if (!ExtractData<TExpected, TLogger>(out var value, logger))
                {
                    if (DataErrors != null)
                    {
                        logger?.LogWarning("Извлечение данных ответа: data errors: {@DataErrors}",
                            Utils.SerializeForLog(DataErrors));
                        return (value, this);
                    }

                    logger?.LogError("Извлечение данных ответа: unknown data error!", Utils.SerializeForLog(DataErrors));
                    return (value, this);
                }

                logger?.LogDebug("Извлечение данных ответа: {@Value}", Utils.SerializeForLog(value));

                return (value, this);
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Извлечение данных ответа thrown exception: {@ExceptionMessage}!", e.Message);
                throw;
            }
        }

        public bool ExtractData<TExpected>(out TExpected value) where TExpected : class =>
            ExtractData<TExpected, ResponseData>(out value, null);

        public bool ExtractData<TExpected, TLogger>(out TExpected value, ILogger<TLogger> logger = null) where TExpected : class
        {
            _dataErrors = null;
            string data = Metadata?.Content;

            logger?.LogDebug($"Извлечение данных ответа: ожидаемый тип {typeof(TExpected)}");

            try
            {
                if (data?.IsValidXml() ?? false)
                {
                    logger?.LogDebug("Извлечение данных ответа: является валидным XML");
                    value = XDocument.Parse(data) as TExpected;
                    return typeof(TExpected) == typeof(XDocument);
                }

                if (data?.IsValidJson() ?? false)
                {
                    logger?.LogDebug("Извлечение данных ответа: является валидным JSON");

                    bool isErrorJson = Utils.ValidateJsonSchemaAs<ErrorResponse, TLogger>(data, logger);
                    DataErrors = isErrorJson ? Utils.DeserializeTo<ErrorResponse>(data) : null;

                    logger?.LogDebug("Извлечение данных ответа: валидация согласно JSON-схеме");

                    bool asExpected = Utils.ValidateJsonSchemaAs<TExpected, TLogger>(data, logger);
                    if (asExpected)
                    {
                        logger?.LogDebug($"Согласно ожиданиям! ({typeof(TExpected)})");

                        value = Utils.DeserializeTo<TExpected>(data) as TExpected;
                        return true;
                    }

                    logger?.LogWarning($"Неожиданный тип ответа!");

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
