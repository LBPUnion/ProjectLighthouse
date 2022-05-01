using System;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;

namespace LBPUnion.ProjectLighthouse.Logging.Loggers;

public class ConsoleLogger : ILogger
{
    public void Log(LogLine logLine)
    {
        ConsoleColor oldBackgroundColor = Console.BackgroundColor;
        ConsoleColor oldForegroundColor = Console.ForegroundColor;

        foreach (string line in logLine.Message.Split('\n'))
        {
            // The following is scuffed. Beware~

            // Write the level! [Success]
            Console.BackgroundColor = logLine.Level.ToColor().ToDark();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write('[');
            Console.ForegroundColor = logLine.Level.ToColor();
            Console.Write(logLine.Level);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(']');
            Console.ForegroundColor = oldForegroundColor;
            Console.BackgroundColor = oldBackgroundColor;
            Console.Write(' ');

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write('<');
            Console.ForegroundColor = logLine.Level.ToColor();
            Console.Write(logLine.Trace.Name);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(':');
            Console.ForegroundColor = logLine.Level.ToColor();
            Console.Write(logLine.Trace.Line);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("> ");
            Console.ForegroundColor = oldForegroundColor;

            Console.WriteLine(line);
        }
    }
}