using System;
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Logging;
using AspLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class LoggingExtensions
{
    public static LogLevel ToLighthouseLevel(this AspLogLevel level)
    {
        return level switch
        {
            AspLogLevel.Trace => LogLevel.Debug,
            AspLogLevel.Debug => LogLevel.Debug,
            AspLogLevel.Information => LogLevel.Info,
            AspLogLevel.Warning => LogLevel.Warning,
            AspLogLevel.Error => LogLevel.Error,
            AspLogLevel.Critical => LogLevel.Error,
            AspLogLevel.None => LogLevel.Info,
            _ => LogLevel.Info,
        };
    }

    public static ConsoleColor ToColor(this LogLevel level) =>
        level switch
        {
            LogLevel.Debug => ConsoleColor.Magenta,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Success => ConsoleColor.Green,
            _ => ConsoleColor.White,
        };

    public static string ToLogString(this IEnumerable<LogLine> log) 
        => log.Aggregate("", (current, logLine) => current + $"[{logLine.Level}] [{logLine.Trace.Name}:{logLine.Trace.Section}] {logLine.Message}\n");

}