using System;
using System.IO;
using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Logging.Loggers;

public class LighthouseFileLogger : ILogger
{
    private static readonly string logsDirectory = Path.Combine(Environment.CurrentDirectory, "logs");

    public void Log(LogLine line)
    {
        FileHelper.EnsureDirectoryCreated(logsDirectory);

        string contentFile = $"[{line.Level}] <{line.Trace.Name}:{line.Trace.Section}> {line.Message}\n";
        string contentAll = $"[{line.Area}:{line.Level}] <{line.Trace.Name}:{line.Trace.Section}> {line.Message}\n";

        try
        {
            File.AppendAllText(Path.Combine(logsDirectory, line.Area + ".log"), contentFile);
            File.AppendAllText(Path.Combine(logsDirectory, "all.log"), contentAll);
        }
        catch(IOException) {} // windows, ya goofed

    }
}