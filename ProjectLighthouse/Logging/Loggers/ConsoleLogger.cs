using System;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Logging;

namespace LBPUnion.ProjectLighthouse.Logging.Loggers;

public class ConsoleLogger : ILogger
{
    public void Log(LogLine logLine)
    {
        ConsoleColor oldForegroundColor = Console.ForegroundColor;
        ConsoleColor logColor = logLine.Level.ToColor();

        foreach (string line in logLine.Message.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            string time = DateTime.Now.ToString("MM/dd/yyyy-HH:mm:ss.fff");
            string trace = "";
            if (logLine.Trace.Name != null)
            {
                trace += logLine.Trace.Name;
                if (logLine.Trace.Section != null) trace += ":" + logLine.Trace.Section;
                trace = "[" + trace + "]";
            }

            Console.ForegroundColor = logColor;
            Console.WriteLine(@$"[{time}] [{logLine.Area}/{logLine.Level.ToString().ToUpper()}] {trace}: {line}");
            Console.ForegroundColor = oldForegroundColor;
        }
    }
}