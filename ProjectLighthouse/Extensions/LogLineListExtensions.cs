using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class LogLineListExtensions
{
    public static string ToLogString(this IEnumerable<LogLine> log) => log.Aggregate(
        "",
        (current, logLine) => current + $"[{logLine.Level}] [{logLine.Trace.Name}:{logLine.Trace.Section}] {logLine.Message}\n"
    );
}