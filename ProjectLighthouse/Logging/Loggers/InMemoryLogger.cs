using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Types.Logging;

namespace LBPUnion.ProjectLighthouse.Logging.Loggers;

public class InMemoryLogger : ILogger
{
    public readonly List<LogLine> Lines = new();
    
    public void Log(LogLine line)
    {
        lock(this.Lines)
        {
            this.Lines.Add(line);
        }
    }
}