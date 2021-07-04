using SsPvo.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SsPvo.Client.Models;

namespace SsPvo.Ui.Common.Batch.Handlers
{
    public class ExcelInportScenarioHandler : IBatchScenarioHandler
    {
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

            try
            {
                var msg = ApiClient.DefaultSsPvoMessageFactory.CreateActionMessage(
                    "get", "ServiceEntrant",
                    XDocument.Parse(
                        $"<PackageData><ServiceEntrant><IDEntrantChoice><SNILS>{pe.Snils}</SNILS></IDEntrantChoice></ServiceEntrant></PackageData>"));

                logger?.LogDebug($"Запрос ServiceEntrant..");

                var response = await ApiClient.SendMessage(msg, token);

                logger?.LogDebug($"Ответ получен..");

                var idJwtResponse = ApiClient.TryExtractResponse<IdJwtResponse>(response);

                // TODO: запрос всех дополнительных сведений

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
        #endregion
    }
}
