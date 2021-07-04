using System.Threading;
using System.Threading.Tasks;

namespace SsPvo.Ui.Common.Batch
{
    public interface IBatchScenarioHandler
    {
        BatchAction.Scenario Scenario { get; }
        Task<BatchAction.Status> ProcessItemAsync(BatchAction.Item item, BatchAction.Options options, CancellationToken token);
    }
}
