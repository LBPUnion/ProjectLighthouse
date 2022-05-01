using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Logging;

// TODO: make into singleton, but with ability to also have instances
// Logger.LogSuccess() should still work and all, but ideally i'm also able to have another instance and do:
// Logger logger = new();
// logger.LogSuccess();
// I should also be able to access the log queue.
// This functionality is going to be used in the admin panel to get the output of commands.
public static class Logger
{

    #region Internals

    /// <summary>
    /// A list of custom loggers to use.
    /// </summary>
    private static readonly List<ILogger> loggers = new();

    public static void AddLogger(ILogger logger)
    {
        loggers.Add(logger);
        LogSuccess("Initialized " + logger.GetType().Name, "Logger");
    }

    private static LogTrace getTrace()
    {
        const int depth = 6;
        const int skipDepth = depth - 2;

        // Get the stacktrace for logging...
        string trace = Environment.StackTrace.Split('\n', depth, StringSplitOptions.RemoveEmptyEntries).Skip(skipDepth).First();

        trace = trace.TrimEnd('\r');
        trace = trace.Substring(trace.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        trace = trace.Replace(".cs:line ", ":");

        string[] traceSplit = trace.Split(':'); // Split for formatting!

        short lineNumber;

        try
        {
            lineNumber = short.Parse(traceSplit[1]);
        }
        catch
        {
            lineNumber = -1;
        }

        return new LogTrace
        {
            Name = traceSplit[0],
            Line = lineNumber,
        };
    }

    #endregion

    #region Queue

    /// <summary>
    /// <para>A queue for the logger.</para>
    /// <para>
    /// We use a queue because if two threads try to log something at the time they'll mess each other's printing up.
    /// </para>
    /// </summary>
    private static readonly ConcurrentQueue<LogLine> logQueue = new();

    /// <summary>
    /// Adds a <see cref="LogLine"/> to the queue. Only used internally.
    /// </summary>
    /// <param name="logLine">The logLine to send to the queue.</param>
    private static void queueLog(LogLine logLine)
    {
        logQueue.Enqueue(logLine);
    }

    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    static Logger() // Start queue thread on first Logger access
    {
        Task.Factory.StartNew
        (
            () =>
            {
                while (true)
                {
                    bool logged = queueLoop();
                    Thread.Sleep(logged ? 10 : 100);
                    // We wait 100ms if we dont log since it's less likely that the program logged again.
                    // If we did log, wait 10ms before looping again.

                    // This is all so we use as little CPU as possible. This is an endless while loop, after all.
                }
            }
        );
    }

    /// <summary>
    /// A function used by the queue thread 
    /// </summary>
    /// <returns></returns>
    private static bool queueLoop()
    {
        bool logged = false;
        if (logQueue.TryDequeue(out LogLine line))
        {
            logged = true;

            foreach (ILogger logger in loggers)
            {
                logger.Log(line);
            }
        }

        return logged;
    }

    #endregion

    #region Logging functions

    public static void LogDebug(string text, string area = "")
    {
        #if DEBUG
        Log(text, area, LogLevel.Debug);
        #endif
    }

    public static void LogSuccess(string text, string area = "")
    {
        Log(text, area, LogLevel.Success);
    }

    public static void LogInfo(string text, string area = "")
    {
        Log(text, area, LogLevel.Info);
    }

    public static void LogWarn(string text, string area = "")
    {
        Log(text, area, LogLevel.Warning);
    }

    public static void LogError(string text, string area = "")
    {
        Log(text, area, LogLevel.Error);
    }

    public static void Log(string text, string area, LogLevel level)
    {
        queueLog
        (
            new LogLine
            {
                Level = level,
                Message = text,
                Area = area,
                Trace = getTrace(),
            }
        );
    }

    #endregion

}