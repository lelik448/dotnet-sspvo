using Serilog;
using SsPvo.Ui;
using System;
using System.Windows.Forms;

namespace SsPvo.Ui
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UiSsPvo(ConfigureLogging()));
            Log.CloseAndFlush();
        }

        static Microsoft.Extensions.Logging.LoggerFactory ConfigureLogging()
        {
            var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .CreateLogger();

            loggerFactory.AddSerilog(loggerConfig);

            return loggerFactory;
        }
    }
}
