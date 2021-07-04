using System.ComponentModel;

namespace SsPvo.Ui.Common.Batch
{
    partial class BatchAction
    {
        public enum Scenario
        {
            [Description("Загрузка данных об абитуриентах")]
            ExcelImport = 1,
        }
    }
}
