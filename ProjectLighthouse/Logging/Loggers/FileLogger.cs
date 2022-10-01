using System;
using System.IO;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Files;

namespace LBPUnion.ProjectLighthouse.Logging.Loggers;

public class FileLogger : ILogger
{
    private static readonly string logsDirectory = Path.Combine(Environment.CurrentDirectory, "logs");

    public void Log(LogLine line)
    {
        FileHelper.EnsureDirectoryCreated(logsDirectory);
        string time = DateTime.Now.ToString("MM/dd/yyyy-HH:mm:ss.fff");
        string contentFile = $"[{time}] [{ServerStatics.ServerType}] [{line.Level}] <{line.Trace.Name}:{line.Trace.Section}> {line.Message}\n";
        string contentAll = $"[{time}] [{ServerStatics.ServerType}] [{line.Area}:{line.Level}] <{line.Trace.Name}:{line.Trace.Section}> {line.Message}\n";

        try
        {
            File.AppendAllText(Path.Combine(logsDirectory, line.Area + ".log"), contentFile);
            File.AppendAllText(Path.Combine(logsDirectory, "all.log"), contentAll);
        }
        catch(IOException) {} // windows, ya goofed

    }
}