using System.Diagnostics.CodeAnalysis;

namespace LBPUnion.ProjectLighthouse.Types.Logging;

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
    Chat,
    HTTP,
    Match,
    Photos,
    Resources,
    Logger,
    Redis,
    Command,
    Admin,
    Publish,
    Maintenance,
    Score,
    RateLimit,
    Deserialization,
    Email,
    Serialization,
    Synchronization,
}