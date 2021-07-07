using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp2
{
    public static class ExcelHelper
    {
        public static SnilsWithEpguIds[] GetSnilsWithAppUidsFromXlsFile(string file)
        {
            using (var excelPackage = new ExcelPackage(new FileInfo(file)))
            {
                string sheetName = "Список заявлений";
                var sheet = excelPackage.Workbook.Worksheets[sheetName];
                if (sheet == null)
                    throw new ArgumentException($"{nameof(file)}: в файле \"{file}\" не найден лист \"{sheetName}\"");

                int numRows = sheet.Dimension.Rows;
                int numCols = sheet.Dimension.Columns;

                var headerToColumnIndex = new Dictionary<string, int>();

                for (int i = 1; i <= numCols; i++)
                {
                    headerToColumnIndex.Add(sheet.Cells[1, i].Value?.ToString(), i);
                }

                var data = new List<XlsxAppInfo>();

                for (int r = 2; r <= numRows; r++)
                {
                    data.Add(new XlsxAppInfo
                    {
                        AppNum = $"{sheet.Cells[r, headerToColumnIndex["Номер заявления"]].Value}",
                        Snils = $"{sheet.Cells[r, headerToColumnIndex["Снилс"]].Value}",
                        EpguId = $"{sheet.Cells[r, headerToColumnIndex["ЕПГУ"]].Value}"
                    });
                }

                return data.GroupBy(x => x.Snils)
                    .Select(gr =>
                    {
                        return new SnilsWithEpguIds
                        {
                            Snils = gr.Key,
                            EpguIds = gr.Select(x => x.EpguId).ToArray()
                        };
                    })
                    .ToArray();
            }
        }

        public struct XlsxAppInfo
        {
            public string AppNum { get; set; }
            public string Snils { get; set; }
            public string EpguId { get; set; }
        }

        public struct SnilsWithEpguIds
        {
            public string Snils { get; set; }
            public string[] EpguIds { get; set; }
        }
    }
}
