using System;
using System.IO;
using System.Windows.Forms;

namespace SsPvo.Ui.Common
{
    public static class DialogHelper
    {
        public static int AskSaveChanges(string message = "Сохранить изменения?", string title = "Выберите действие")
        {
            switch (MessageBox.Show($@"{message}", title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
            {
                case DialogResult.Yes:
                    return 1;
                case DialogResult.No:
                    return 2;
                default:
                    return 0;
            }
        }

        public static bool AskConfirmation(string message, string title = "Подтвердите действие")
        {
            return (MessageBox.Show($@"{message}", title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    DialogResult.Yes);
        }

        public static void ShowError(string message, string title = "Ошибка!")
        {
            MessageBox.Show($@"{message}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowInfo(string message, string title = "Информация")
        {
            MessageBox.Show($@"{message}", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowWarning(string message, string title = "Внимание!")
        {
            MessageBox.Show($@"{message}", title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowInsufficientRights(string message = "Недостаточно прав для выполнения операции!", string title = "Внимание!")
        {
            MessageBox.Show($@"{message}", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        public static object OpenFile(
            string extFilter,
            string defaultExt = null,
            string initialDirectory = null,
            Environment.SpecialFolder initialDirectoryFallback = Environment.SpecialFolder.Desktop,
            bool multyselect = false,
            int sizeLimit = 0,
            bool checkFileExists = true)
        {
            using (var ofd = new OpenFileDialog()
            {
                CheckFileExists = checkFileExists,
                CheckPathExists = checkFileExists,
                DefaultExt = defaultExt ?? string.Empty,
                Filter = extFilter ?? "Все файлы | *.*",
                InitialDirectory = !string.IsNullOrWhiteSpace(initialDirectory)
                    ? new FileInfo(initialDirectory).Directory?.FullName
                    : Environment.GetFolderPath(initialDirectoryFallback),
                Multiselect = multyselect,
                SupportMultiDottedExtensions = true
            })
            {
                if (ofd.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(ofd.FileName)) return null;
                if (File.Exists(ofd.FileName)
                    && sizeLimit <= 0
                    || (sizeLimit > 0 && new FileInfo(ofd.FileName).Length <= sizeLimit))
                {
                    return ofd.Multiselect ? (object)ofd.FileNames : ofd.FileName;
                }
            }

            return null;
        }

        public static string OpenPath(
            string initialDirectory = null,
            bool showNewFolderButton = true,
            Environment.SpecialFolder rootFolder = Environment.SpecialFolder.MyComputer,
            Environment.SpecialFolder initialDirectoryFallback = Environment.SpecialFolder.Desktop)
        {
            using (var fbd = new FolderBrowserDialog()
            {
                RootFolder = rootFolder,
                ShowNewFolderButton = showNewFolderButton,
                SelectedPath = initialDirectory ?? Environment.GetFolderPath(initialDirectoryFallback)
            })
            {
                if (fbd.ShowDialog() == DialogResult.OK
                    && !string.IsNullOrWhiteSpace(fbd.SelectedPath)
                    && Directory.Exists(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                }
            }

            return null;
        }
    }
}
