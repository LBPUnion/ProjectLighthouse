using System;
using Microsoft.Extensions.Logging;
using IAspLogger = Microsoft.Extensions.Logging.ILogger;

namespace LBPUnion.ProjectLighthouse.Logging.Loggers.AspNet;

[ProviderAlias("Kettu")]
public class AspNetToLighthouseLoggerProvider : ILoggerProvider, IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public IAspLogger CreateLogger(string category) => new AspNetToLighthouseLogger(category);
}