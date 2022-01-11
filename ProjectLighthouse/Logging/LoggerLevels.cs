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

    public class LoggerLevelLogin : LoggerLevel
    {
        public static readonly LoggerLevelLogin Instance = new();
        public override string Name => "Login";
    }

    public class LoggerLevelResources : LoggerLevel
    {
        public static readonly LoggerLevelResources Instance = new();
        public override string Name => "Resources";
    }

    public class LoggerLevelMatch : LoggerLevel
    {
        public static readonly LoggerLevelMatch Instance = new();
        public override string Name => "Match";
    }

    public class LoggerLevelPhotos : LoggerLevel
    {
        public static readonly LoggerLevelPhotos Instance = new();
        public override string Name => "Photos";
    }

    public class LoggerLevelConfig : LoggerLevel
    {
        public static readonly LoggerLevelConfig Instance = new();
        public override string Name => "Config";
    }

    public class LoggerLevelInflux : LoggerLevel
    {
        public static readonly LoggerLevelInflux Instance = new();
        public override string Name => "Influx";
    }

    public class LoggerLevelAspNet : LoggerLevel
    {

        public LoggerLevelAspNet(LogLevel level)
        {
            this.Channel = level.ToString();
        }
        public override string Name => "AspNet";
    }

    public class LoggerLevelCategory : LoggerLevel
    {
        public static readonly LoggerLevelCategory Instance = new();
        public override string Name => "Category";
    }
}