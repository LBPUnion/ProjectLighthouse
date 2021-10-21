using System;
using Microsoft.Extensions.Logging;

namespace LBPUnion.ProjectLighthouse.Logging {
    [ProviderAlias("Kettu")]
    public class AspNetToKettuLoggerProvider : ILoggerProvider, IDisposable {
        public void Dispose() {
            // cry about it
        }
        
        public ILogger CreateLogger(string categoryName) {
            return new AspNetToKettuLogger();
        }
    }
}