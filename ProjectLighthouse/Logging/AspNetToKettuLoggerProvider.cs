using System;
using Microsoft.Extensions.Logging;

namespace LBPUnion.ProjectLighthouse.Logging
{
    [ProviderAlias("Kettu")]
    public class AspNetToKettuLoggerProvider : ILoggerProvider, IDisposable
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public ILogger CreateLogger(string categoryName) => new AspNetToKettuLogger();
    }
}