using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SsPvo.Ui.Extensions
{
    public static class DataGridViewExtensions
    {
        /// <summary>
        /// рисует номера строк
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="font"></param>
        /// <param name="e"></param>
        public static void PaintRowNumbers(this DataGridView grid, Font font, DataGridViewRowPostPaintEventArgs e)
        {
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        /// <summary>
        /// возвращает первую выбранную строку или null
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static DataGridViewRow FirstSelectedRow(this DataGridView grid) =>
            grid.SelectedRows.OfType<DataGridViewRow>().FirstOrDefault();

        /// <summary>
        /// возвращает DataBoundItem в виде object первой выбранной строки
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static object FirstSelectedRowDataBoundItem(this DataGridView grid) =>
            grid.SelectedRows.OfType<DataGridViewRow>().FirstOrDefault()?.DataBoundItem;

        /// <summary>
        /// возвращает DataBoundItem'ы в виде object для всех выбранных строк
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static object SelectedRowsDataBoundItems(this DataGridView grid) => 
            grid.SelectedRows.OfType<DataGridViewRow>()
                .Select(r => r.DataBoundItem).ToList();

        /// <summary>
        /// возвращает DataBoundItem'ы в виде IEnumerable T для всех выбранных строк
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static IEnumerable<T> SelectedRowsDataBoundItems<T>(this DataGridView grid)
            where T : class
        {
            return grid.SelectedRows.OfType<DataGridViewRow>()
                .Select(r => r.DataBoundItem as T)
                .ToList();
        }
    }
}
