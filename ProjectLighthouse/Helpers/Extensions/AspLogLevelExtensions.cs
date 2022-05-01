using LBPUnion.ProjectLighthouse.Logging;
using AspLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace LBPUnion.ProjectLighthouse.Helpers.Extensions;

public static class AspLogLevelExtensions
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
}