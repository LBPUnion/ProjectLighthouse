using Kettu;

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

    #region ASP.NET
    public class LoggerLevelAspNetTrace : LoggerLevel {
        public override string Name => "ASP.NET: Trace";
        public static readonly LoggerLevelAspNetTrace Instance = new();
    }
    
    public class LoggerLevelAspNetDebug : LoggerLevel {
        public override string Name => "ASP.NET: Debug";
        public static readonly LoggerLevelAspNetDebug Instance = new();
    }
    
    public class LoggerLevelAspNetInformation : LoggerLevel {
        public override string Name => "ASP.NET: Information";
        public static readonly LoggerLevelAspNetInformation Instance = new();
    }
    
    public class LoggerLevelAspNetWarning : LoggerLevel {
        public override string Name => "ASP.NET: Warning";
        public static readonly LoggerLevelAspNetWarning Instance = new();
    }
    
    public class LoggerLevelAspNetError : LoggerLevel {
        public override string Name => "ASP.NET: Error";
        public static readonly LoggerLevelAspNetError Instance = new();
    }
    
    public class LoggerLevelAspNetCritical : LoggerLevel {
        public override string Name => "ASP.NET: Critical";
        public static readonly LoggerLevelAspNetCritical Instance = new();
    }
    
    public class LoggerLevelAspNetNone : LoggerLevel {
        public override string Name => "ASP.NET: None";
        public static readonly LoggerLevelAspNetNone Instance = new();
    }
    #endregion
}