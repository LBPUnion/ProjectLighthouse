using Kettu;
using Microsoft.Extensions.Logging;

namespace LBPUnion.ProjectLighthouse.Logging {
    public class LoggerLevelStartup : LoggerLevel {
        public override string Name => "Startup";
        public static readonly LoggerLevelStartup Instance = new();
    }

    public class LoggerLevelDatabase : LoggerLevel {
        public override string Name => "Database";
        public static readonly LoggerLevelDatabase Instance = new();
    }

    public class LoggerLevelHttp : LoggerLevel {
        public override string Name => "HTTP";
        public static readonly LoggerLevelHttp Instance = new();
    }
    
    public class LoggerLevelFilter : LoggerLevel {
            public override string Name => "Filter";
            public static readonly LoggerLevelFilter Instance = new();
        }

    public class LoggerLevelAspNet : LoggerLevel {
        public override string Name => "AspNet";
        
        public LoggerLevelAspNet(LogLevel level) {
            this.Channel = level.ToString();
        }
    }
}