#nullable enable
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
        LogSuccess("Initialized " + logger.GetType().Name, LogArea.Logger);
    }

    private static LogTrace getTrace(int extraTraceLines = 0)
    {
        const int depth = 5;
        const int skipDepth = depth - 2;

//        // Get the stacktrace for logging...
//        string trace = Environment.StackTrace
//            .Split('\n', depth + extraTraceLines, StringSplitOptions.RemoveEmptyEntries)
//            .Skip(skipDepth + extraTraceLines)
//            .First();
//
//        trace = trace.TrimEnd('\r');
//        trace = trace.Substring(trace.LastIndexOf(Path.DirectorySeparatorChar) + 1); // Try splitting by the filename.
//        if (trace.StartsWith("   at ")) // If we still havent split properly...
//        {
//            trace = trace.Substring(trace.LastIndexOf('.') + 1); // Try splitting by the last dot.
//        }
//        trace = trace.Replace(".cs:line ", ":");

        StackTrace stackTrace = new(true);
        StackFrame? frame = stackTrace.GetFrame(skipDepth + extraTraceLines);
        Debug.Assert(frame != null);

        string? name;
        string? section;

        string? fileName = frame.GetFileName();
        if (fileName == null)
        {
            name = frame.GetMethod()?.DeclaringType?.Name;
            section = frame.GetMethod()?.Name;
        }
        else
        {
            name = Path.GetFileNameWithoutExtension(Path.GetFileName(fileName));
            int lineNumber = frame.GetFileLineNumber();
            if (lineNumber == 0) lineNumber = -1;

            section = lineNumber.ToString();
        }

        return new LogTrace
        {
            Name = name,
            Section = section,
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

    public static void LogDebug(string text, LogArea logArea)
    {
        #if DEBUG
        Log(text, logArea.ToString(), LogLevel.Debug);
        #endif
    }

    public static void LogSuccess(string text, LogArea logArea)
    {
        Log(text, logArea.ToString(), LogLevel.Success);
    }

    public static void LogInfo(string text, LogArea logArea)
    {
        Log(text, logArea.ToString(), LogLevel.Info);
    }

    public static void LogWarn(string text, LogArea logArea)
    {
        Log(text, logArea.ToString(), LogLevel.Warning);
    }

    public static void LogError(string text, LogArea logArea)
    {
        Log(text, logArea.ToString(), LogLevel.Error);
    }

    public static void Log(string text, string area, LogLevel level, int extraTraceLines = 0)
    {
        queueLog
        (
            new LogLine
            {
                Level = level,
                Message = text,
                Area = area,
                Trace = getTrace(extraTraceLines),
            }
        );
    }

    #endregion

}