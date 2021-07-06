using SsPvo.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SsPvo.Client.Enums;
using SsPvo.Client.Models;
using SsPvo.Client.Messages;
using SsPvo.Client.Messages.Serialization;

namespace SsPvo.Ui.Common.Batch.Handlers
{
    public class ExcelInportScenarioHandler : IBatchScenarioHandler
    {
        #region fields
        // private readonly ConcurrentQueue<SsPvoMessage> _msgQueue;
        private string savePathBase = "files";
        #endregion

        #region ctor
        public ExcelInportScenarioHandler(SsPvoApiClient apiClient)
        {
            ApiClient = apiClient;
        }
        #endregion

        #region props
        public SsPvoApiClient ApiClient { get; set; }
        public BatchAction.Scenario Scenario => BatchAction.Scenario.ExcelImport;
        #endregion

        #region methods
        public async Task<BatchAction.Status> ProcessItemAsync(BatchAction.Item item, BatchAction.Options options, CancellationToken token)
        {
            if (ApiClient == null) return item.SetResult(BatchAction.Status.Error, "Клиент API недоступен!");

            var logger = options.GetValueOrDefault<ILogger<BatchAction>>($"{BatchAction.Options.CommonOptions.Log}");

            logger?.LogDebug($"Абитуриент {item.Description}..");

            var pe = item.GetProcessedEntity<SsAppFromExcel>();

            // TODO: token.ThrowIfCancellationRequested();

            try
            {
                // 1. запрос по СНИЛС
                logger?.LogDebug($"Запрос serviceEntrant..");

                var msgServiceEntrant = ApiClient.DefaultSsPvoMessageFactory.CreateActionMessage(
                    "get", "serviceEntrant",
                    XDocument.Parse(
                        $"<PackageData><ServiceEntrant><IDEntrantChoice><SNILS>{pe.Snils}</SNILS></IDEntrantChoice></ServiceEntrant></PackageData>"));
                
                await ApiClient.SendMessage(msgServiceEntrant, token);

                logger?.LogDebug($"Ответ serviceEntrant получен. Извлекаем данные..");

                var idJwtResponse = ApiClient.TryExtractResponse<IdJwtResponse>(msgServiceEntrant.ResponseData);

                if (string.IsNullOrWhiteSpace(idJwtResponse.Item1?.IdJwt))
                {
                    return item.SetResult(BatchAction.Status.Error,
                        $"Студент {pe.FullName}[СНИЛС:{pe.Snils}] ошибка получения idJwt для запроса профиля!");
                }

                logger?.LogDebug($"idJwt: {idJwtResponse.Item1.IdJwt}");

                // 2. запрос профиля по idJwt
                logger?.LogDebug($"Запрос запрос профиля абитуриента по idJwt {idJwtResponse.Item1.IdJwt} из service..");

                var msgServiceEntrantProfile =
                    ApiClient.DefaultSsPvoMessageFactory.CreateGetQueueItemMessage(SsPvoQueue.Service,
                        int.Parse(idJwtResponse.Item1.IdJwt));

                await ApiClient.SendMessage(msgServiceEntrantProfile, token);

                logger?.LogDebug($"Ответ запроса профиля получен. Извлекаем данные..");

                var msgServiceEntrantProfileTokenResponse =
                    ApiClient.TryExtractResponse<ResponseTokenResponse>(msgServiceEntrantProfile.ResponseData);

                if (string.IsNullOrWhiteSpace(msgServiceEntrantProfileTokenResponse.Item1?.ResponseToken))
                {
                    return item.SetResult(BatchAction.Status.Error,
                        $"Студент {pe.FullName}[СНИЛС:{pe.Snils}] ошибка получения профиля абитуриента по idJwt {idJwtResponse.Item1.IdJwt}!");
                }

                var xDocProfile = new JwtToken(msgServiceEntrantProfileTokenResponse.Item1.ResponseToken).Decode().Item2;

                if (xDocProfile != null)
                {
                    EnsureFolderExist();
                    string fullPath = Path.GetFullPath(Path.Combine(savePathBase, $"{pe.FullName} ({pe.Snils}).xml"));
                    logger?.LogDebug($"Сохраняем файл \"{fullPath}\"");
                    xDocProfile.Save(fullPath);
                }

                // TODO: запросы дополнительных сведений

                return item.SetResult(BatchAction.Status.Completed,
                    $"Студент {pe.FullName}[СНИЛС:{pe.Snils}] обработка успешно завершена!");
            }
            catch (OperationCanceledException e)
            {
                return item.SetResult(BatchAction.Status.Canceled,
                    $"Студент {pe.FullName}[СНИЛС:{pe.Snils}] обработка прервана! {e.Message}");
            }
            catch (Exception e)
            {
                return item.SetResult(BatchAction.Status.Error, e.Message);
            }
        }

        private void EnsureFolderExist()
        {
            if (!Directory.Exists(savePathBase)) Directory.CreateDirectory(savePathBase);
        }
        #endregion
    }
}
