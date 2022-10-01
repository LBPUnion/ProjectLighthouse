using System;
using LBPUnion.ProjectLighthouse.Extensions;

namespace LBPUnion.ProjectLighthouse.Logging.Loggers;

public class ConsoleLogger : ILogger
{
    public void Log(LogLine logLine)
    {
        ConsoleColor oldForegroundColor = Console.ForegroundColor;

        foreach (string line in logLine.Message.Split('\n'))
        {
            // The following is scuffed.
            // Beware~

            string time = DateTime.Now.ToString("MM/dd/yyyy-HH:mm:ss.fff");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write('[');
            Console.ForegroundColor = logLine.Level.ToColor();
            Console.Write(time);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(']');
            Console.Write(' ');

            // Write the level! [Success]
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write('[');
            Console.ForegroundColor = logLine.Level.ToColor();
            Console.Write(logLine.Area);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(':');
            Console.ForegroundColor = logLine.Level.ToColor();
            Console.Write(logLine.Level);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(']');
            Console.ForegroundColor = oldForegroundColor;
            Console.Write(' ');

            if (logLine.Trace.Name != null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write('<');
                Console.ForegroundColor = logLine.Level.ToColor();
                Console.Write(logLine.Trace.Name);
                if (logLine.Trace.Section != null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(':');
                    Console.ForegroundColor = logLine.Level.ToColor();
                    Console.Write(logLine.Trace.Section);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write('>');
                Console.Write(' ');
                Console.ForegroundColor = oldForegroundColor;
            }

            Console.WriteLine(line);
        }
    }
}