using System;
using LBPUnion.ProjectLighthouse.Extensions;
using Microsoft.Extensions.Logging;
using AspLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace LBPUnion.ProjectLighthouse.Logging.Loggers.AspNet;

public class AspNetToLighthouseLogger : Microsoft.Extensions.Logging.ILogger
{
    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
    public bool IsEnabled(AspLogLevel logLevel) => true;

    private string Category { get; init; }

    public AspNetToLighthouseLogger(string category)
    {
        this.Category = category;
    }

    public void Log<TState>(AspLogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        LogLevel level = logLevel.ToLighthouseLevel();

        Logger.Instance.Log(state.ToString() ?? string.Empty, this.Category, level, 4);

        if (exception == null) return;

        Logger.Instance.Log(exception.ToDetailedException(), this.Category, level, 4);
    }
}