using System;
using Kettu;
using Microsoft.Extensions.Logging;

namespace LBPUnion.ProjectLighthouse.Logging {
    public class AspNetToKettuLogger : ILogger {

        public IDisposable BeginScope<TState>(TState state) {
            return NullScope.Instance;
        }
        public bool IsEnabled(LogLevel logLevel) => true;
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            LoggerLevel loggerLevel = logLevel switch {

                LogLevel.Trace => LoggerLevelAspNetTrace.Instance,
                LogLevel.Debug => LoggerLevelAspNetDebug.Instance,
                LogLevel.Information => LoggerLevelAspNetInformation.Instance,
                LogLevel.Warning => LoggerLevelAspNetWarning.Instance,
                LogLevel.Error => LoggerLevelAspNetError.Instance,
                LogLevel.Critical => LoggerLevelAspNetCritical.Instance,
                LogLevel.None => LoggerLevelAspNetNone.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null),
            };
            
            Logger.Log(state.ToString(), loggerLevel);
            if(exception != null) {
                Logger.Log(exception.ToString(), loggerLevel);
            }
        }
    }
}