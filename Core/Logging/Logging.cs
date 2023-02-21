
namespace Lithium.Logging
{
    using System;
    public class Log
    {
        public readonly object Source = default!;

        public Log(object source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        private Action<object?, object?>? infoEvent, errorEvent, warnEvent;
        public void Info(object message) { infoEvent?.Invoke(Source, message); }
        public void Warn(object message) { warnEvent?.Invoke(Source,message); }
        public void Error(object message) { errorEvent?.Invoke(Source,message); }

        public void AddErrorHandler(Action<object?, object?> errorHandler) => errorEvent += errorHandler;
        public void RemoveErrorHandler(Action<object?, object?> errorHandler) => errorEvent -= errorHandler;

        public void AddWarnHandler(Action<object?, object?> warnHandler) => warnEvent += warnHandler;
        public void RemoveWarnHandler(Action<object?, object?> warnHandler) => warnEvent -= warnHandler;

        public void AddInfoHandler(Action<object?, object?> infoHandler) => infoEvent += infoHandler;
        public void RemoveInfoHandler(Action<object?, object?> infoHandler) => infoEvent -= infoHandler;
    }
}
