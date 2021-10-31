using System;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;
using Microsoft.Extensions.Logging;

namespace LBPUnion.ProjectLighthouse.Logging
{
    public class AspNetToKettuLogger : ILogger
    {

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LoggerLevel loggerLevel = new LoggerLevelAspNet(logLevel);

            Logger.Log(state.ToString(), loggerLevel);
            if (exception == null) return;

            string[] lines = exception.ToDetailedException().Replace("\r", "").Split("\n");
            foreach (string line in lines) Logger.Log(line, loggerLevel);
        }
    }
}