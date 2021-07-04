using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SsPvo.Ui.Extensions
{
    public static class FormExtensions
    {
        /// <summary>
        /// Индикация статуса (версия для loader.gif)
        /// </summary>
        /// <param name="form"></param>
        /// <param name="busy"></param>
        /// <param name="message"></param>
        /// <param name="label"></param>
        /// <param name="loaderIcon"></param>
        public static void IndicateState(
            this Form form,
            bool busy,
            string message = "",
            ToolStripStatusLabel label = null,
            ToolStripStatusLabel loaderIcon = null)
        {
            // status strip
            var strip = label?.GetCurrentParent();

            // status label
            if (label != null)
            {
                strip?.Invoke((Action)(() => label.Text = $@"{message}"));
            }

            // loader gif
            if (loaderIcon != null)
            {
                if (busy)
                {
                    // resetting image (animation reset)
                    var tmp = loaderIcon.Image;
                    loaderIcon.Image = null;
                    strip?.Invoke((Action)(() => loaderIcon.Image = tmp));
                }

                strip?.Invoke((Action) (() => loaderIcon.Visible = busy));
            }

            // cursor
            form.Invoke((Action) (() => form.Cursor = (busy ? Cursors.WaitCursor : Cursors.Default)));

            Application.DoEvents();
        }
    }
}
