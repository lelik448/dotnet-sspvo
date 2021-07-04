namespace SsPvo.Ui.Common.Logging
{
    public static class LogUtils
    {
        public static CustomLogEventSink CustomLogEventSink { get; } = new CustomLogEventSink();
    }
}
