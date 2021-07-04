using Serilog.Core;
using Serilog.Events;
using System;

namespace SsPvo.Ui.Common.Logging
{
    public class CustomLogEventSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            OnLogEvent(logEvent);
        }

        public event EventHandler<LogEvent> LogEvent;

        protected virtual void OnLogEvent(LogEvent e)
        {
            LogEvent?.Invoke(this, e);
        }
    }
}
