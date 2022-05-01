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

        string channel = string.IsNullOrEmpty(line.Area) ? "" : $"[{line.Area}] ";

        string contentFile = $"{channel}{line.Message}\n";
        string contentAll = $"[{$"{line.Level} {channel}".TrimEnd()}] {line.Message}\n";

        try
        {
            File.AppendAllText(Path.Combine(logsDirectory, line.Level + ".log"), contentFile);
            File.AppendAllText(Path.Combine(logsDirectory, "all.log"), contentAll);
        }
        catch(IOException) {} // windows, ya goofed

    }
}