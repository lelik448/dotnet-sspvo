using System.ComponentModel;

namespace SsPvo.Ui.Common.Batch
{
    partial class BatchAction
    {
        public enum Status
        {
            [Description("Не начато")]
            NotStarted = 1,
            [Description("В обработке")]
            InProgress = 2,
            [Description("Пауза")]
            Paused = 3,
            [Description("Завершено")]
            Completed = 4,
            [Description("Отменено")]
            Canceled = 5,
            [Description("Ошибка")]
            Error = 6
        }
    }
}
