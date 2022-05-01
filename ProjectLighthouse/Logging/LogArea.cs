using System.Diagnostics.CodeAnalysis;

namespace LBPUnion.ProjectLighthouse.Logging;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum LogArea
{
    Login,
    Startup,
    Category,
    Comments,
    Config,
    Database,
    Filter,
    HTTP,
    InfluxDB,
    Match,
    Photos,
    Resources,
    Logger,
}