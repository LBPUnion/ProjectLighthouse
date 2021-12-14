using System;
using System.IO;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;

namespace LBPUnion.ProjectLighthouse.Logging
{
    public class LighthouseFileLogger : LoggerBase
    {
        private static readonly string logsDirectory = Path.Combine(Environment.CurrentDirectory, "logs");
        public override bool AllowMultiple => false;

        public override void Send(LoggerLine line)
        {
            FileHelper.EnsureDirectoryCreated(logsDirectory);

            string channel = string.IsNullOrEmpty(line.LoggerLevel.Channel) ? "" : $"[{line.LoggerLevel.Channel}] ";

            string contentFile = $"{channel}{line.LineData}\n";
            string contentAll = $"[{$"{line.LoggerLevel.Name} {channel}".TrimEnd()}] {line.LineData}\n";

            try
            {
                File.AppendAllText(Path.Combine(logsDirectory, line.LoggerLevel.Name.ToFileName() + ".log"), contentFile);
                File.AppendAllText(Path.Combine(logsDirectory, "all.log"), contentAll);
            }
            catch(IOException) {} // windows, ya goofed

        }
    }
}