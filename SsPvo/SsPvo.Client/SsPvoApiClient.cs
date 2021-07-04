using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SsPvo.Client.Enums;
using SsPvo.Client.Messages;
using SsPvo.Client.Messages.Exceptions;
using SsPvo.Client.Messages.Serialization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SsPvo.Client.Models;

namespace SsPvo.Client
{
    public class SsPvoApiClient
    {
        #region fields
        private readonly ICsp _csp;
        private readonly ILogger<SsPvoApiClient> _logger;
        private readonly RestClient _restClient;
        
        private readonly IReadOnlyDictionary<SsPvoMessageType, string> _apiPaths =
            new Dictionary<SsPvoMessageType, string>
            {
                // используется для получения справочников (синхронный метод)
                { SsPvoMessageType.Cls, "cls/request" },
                // используется для проверки действительности регистрации сертификата в сервисе приема (синхронный метод)
                { SsPvoMessageType.Cert, "certificate/check" },
                // используются для загрузки сообщений (данных) в систему (асинхронный метод)
                { SsPvoMessageType.Action, "token/new" },
                // используется для работы с очередью, содержащей сообщения из ЕПГУ для ИС ООВО
                { SsPvoMessageType.ServiceQueue, "token/service/info" },
                // используется для работы с очередью, содержащей результаты обработки сообщений (данных), загруженных в сервис приема ИС ООВО (синхронный метод)
                { SsPvoMessageType.EpguQueue, "token/epgu/info" },
                // используется для подтверждения получения ООВО сообщений из очередей (синхронный метод)
                { SsPvoMessageType.Confirm, "token/confirm" }
            };
        #endregion

        public SsPvoApiClient(string ogrn, string kpp, string apiUrl, ICsp csp, ILogger<SsPvoApiClient> logger)
        {
            if (string.IsNullOrWhiteSpace(ogrn)) throw new ArgumentNullException(nameof(ogrn));
            if (string.IsNullOrWhiteSpace(kpp)) throw new ArgumentNullException(nameof(kpp));
            if (string.IsNullOrWhiteSpace(apiUrl)) throw new ArgumentNullException(nameof(apiUrl));

            _csp = csp ?? throw new ArgumentNullException(nameof(csp));
            _logger = logger;
            _restClient = new RestClient(apiUrl);
            _restClient.UseNewtonsoftJson(Utils.SerializerSettings);

            DefaultSsPvoMessageFactory = new SsPvoSsPvoMessageFactory(ogrn, kpp);

            logger?.LogDebug($"{nameof(SsPvoApiClient)} initialized");
        }

        #region props
        public SsPvoSsPvoMessageFactory DefaultSsPvoMessageFactory { get; }
        #endregion

        #region methods
        public virtual async Task<ResponseData> SendMessage(SsPvoMessage.Options options,
            CancellationToken token = default(CancellationToken)) =>
            await SendMessage(DefaultSsPvoMessageFactory.Create(options), token);

        public virtual async Task<ResponseData> SendMessage(SsPvoMessage msg, 
            CancellationToken token = default(CancellationToken))
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            IRestRequest request = null;
            IRestResponse response = null;

            try
            {
                if (msg.RequestData.Prepared == null) msg.PrepareRequestData(_csp);
                if (_restClient == null) throw new InvalidOperationException($"{nameof(_restClient)} is null");
                if (msg.RequestData.Prepared == null) throw new InvalidOperationException("message is not prepared!");

                request = new RestRequest(_apiPaths[msg.MessageType]).AddJsonBody(msg.RequestData.Prepared);

                _logger?.LogDebug("Sending message '{@MessageGuid}' {@JsonRequest} to {@Url}",
                    msg.Guid,
                    Utils.SerializeForLog(msg.RequestData.Prepared),
                    $"{_restClient?.BaseUrl?.AbsoluteUri}/{request.Resource}");

                response = await SendRestRequest(request, token);
                msg.ResponseData = new ResponseData(msg, ResponseMetadata.From(response));
            }
            catch (OperationCanceledException e)
            {
                _logger?.LogWarning(e, $"Операция была прервана. '{msg.Guid}'. {e.Message}");
                throw;
            }
            catch (SsPvoMessageRequestPreparationException e)
            {
                _logger?.LogError(e, $"Ошибка при подготовке сообщения '{msg.Guid}' к отправке: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"Ошибка при отправке сообщения '{msg.Guid}': {e.Message}");
                throw;
            }
            finally
            {
                Utils.LogRequest(msg.Guid, _restClient, _logger, request, response);
            }

            return msg.ResponseData;
        }

        public virtual async Task<IRestResponse> SendRestRequest(string url, object messageBody, 
            CancellationToken token = default(CancellationToken))
        {
            var request = new RestRequest(url).AddJsonBody(messageBody);
            return await SendRestRequest(request, token);
        }

        public virtual async Task<IRestResponse> SendRestRequest(IRestRequest request, 
            CancellationToken token = default(CancellationToken))
        {
            return await _restClient.ExecutePostAsync(request, token);
        }

        public ValueTuple<TExpected, ResponseData> TryExtractResponse<TExpected>(ResponseData rd)
            where TExpected : class
        {
            return rd.TryExtractResponse<TExpected, SsPvoApiClient>(_logger);
        }
        #endregion
    }
}
