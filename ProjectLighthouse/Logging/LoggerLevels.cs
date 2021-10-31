using Kettu;
using Microsoft.Extensions.Logging;

namespace LBPUnion.ProjectLighthouse.Logging
{
    public class LoggerLevelStartup : LoggerLevel
    {
        public static readonly LoggerLevelStartup Instance = new();
        public override string Name => "Startup";
    }

    public class LoggerLevelDatabase : LoggerLevel
    {
        public static readonly LoggerLevelDatabase Instance = new();
        public override string Name => "Database";
    }

    public class LoggerLevelHttp : LoggerLevel
    {
        public static readonly LoggerLevelHttp Instance = new();
        public override string Name => "HTTP";
    }

    public class LoggerLevelFilter : LoggerLevel
    {
        public static readonly LoggerLevelFilter Instance = new();
        public override string Name => "Filter";
    }

    public class LoggerLevelAspNet : LoggerLevel
    {

        public LoggerLevelAspNet(LogLevel level)
        {
            this.Channel = level.ToString();
        }
        public override string Name => "AspNet";
    }
}