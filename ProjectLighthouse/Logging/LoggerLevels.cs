using Kettu;

namespace LBPUnion.ProjectLighthouse.Logging {
    public class LoggerLevelStartup : LoggerLevel {
        public override string Name => "Startup";
        public static LoggerLevelStartup Instance = new();
    }

    public class LoggerLevelDatabase : LoggerLevel {
        public override string Name => "Database";
        public static LoggerLevelDatabase Instance = new();
    }

    #region ASP.NET
    public class LoggerLevelAspNetTrace : LoggerLevel {
        public override string Name => "ASP.NET: Trace";
        public static LoggerLevelAspNetTrace Instance = new();
    }
    
    public class LoggerLevelAspNetDebug : LoggerLevel {
        public override string Name => "ASP.NET: Debug";
        public static LoggerLevelAspNetDebug Instance = new();
    }
    
    public class LoggerLevelAspNetInformation : LoggerLevel {
        public override string Name => "ASP.NET: Information";
        public static LoggerLevelAspNetInformation Instance = new();
    }
    
    public class LoggerLevelAspNetWarning : LoggerLevel {
        public override string Name => "ASP.NET: Warning";
        public static LoggerLevelAspNetWarning Instance = new();
    }
    
    public class LoggerLevelAspNetError : LoggerLevel {
        public override string Name => "ASP.NET: Error";
        public static LoggerLevelAspNetError Instance = new();
    }
    
    public class LoggerLevelAspNetCritical : LoggerLevel {
        public override string Name => "ASP.NET: Critical";
        public static LoggerLevelAspNetCritical Instance = new();
    }
    
    public class LoggerLevelAspNetNone : LoggerLevel {
        public override string Name => "ASP.NET: None";
        public static LoggerLevelAspNetNone Instance = new();
    }
    #endregion
}