#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Logging;

public class Logger
{
    internal static readonly Logger Instance = new();
    
    #region Internals

    /// <summary>
    /// A list of custom loggers to use.
    /// </summary>
    private readonly List<ILogger> loggers = new();

    public void AddLogger(ILogger logger)
    {
        this.loggers.Add(logger);
        this.LogDebug("Initialized " + logger.GetType().Name, LogArea.Logger);
    }

    private static LogTrace getTrace(int extraTraceLines = 0)
    {
        const int depth = 5;
        const int skipDepth = depth - 2;

        StackTrace stackTrace = new(true);
        StackFrame? frame = stackTrace.GetFrame(skipDepth + extraTraceLines);
        System.Diagnostics.Debug.Assert(frame != null);

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
    private readonly ConcurrentQueue<LogLine> logQueue = new();

    /// <summary>
    /// Adds a <see cref="LogLine"/> to the queue. Only used internally.
    /// </summary>
    /// <param name="logLine">The logLine to send to the queue.</param>
    private void queueLog(LogLine logLine)
    {
        this.logQueue.Enqueue(logLine);
    }

    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public Logger() // Start queue thread on first Logger access
    {
        Task.Factory.StartNew
        (
            () =>
            {
                while (true)
                {
                    bool logged = this.queueLoop();
                    Thread.Sleep(logged ? 10 : 100);
                    // We wait 100ms if we dont log since it's less likely that the program logged again.
                    // If we did log, wait 10ms before looping again.

                    // This is all so we use as little CPU as possible. This is an endless while loop, after all.
                }
            }
        );

        // Flush the log queue when we're exiting.
        AppDomain.CurrentDomain.UnhandledException += this.Flush;
        AppDomain.CurrentDomain.ProcessExit += this.Flush;
    }

    /// <summary>
    /// Logs everything in the queue to all loggers immediately.
    /// This is a helper function to allow for this function to be easily added to events.
    /// </summary>
    private void Flush(object? _, EventArgs __)
    {
        this.Flush();
    }

    /// <summary>
    /// Logs everything in the queue to all loggers immediately.
    /// </summary>
    public void Flush()
    {
        while (this.logQueue.TryDequeue(out LogLine line))
        {
            foreach (ILogger logger in this.loggers)
            {
                logger.Log(line);
            }
        }
    }

    /// <summary>
    /// A function used by the queue thread 
    /// </summary>
    /// <returns></returns>
    private bool queueLoop()
    {
        if (!this.logQueue.TryDequeue(out LogLine line)) return false;

        foreach (ILogger logger in this.loggers)
        {
            logger.Log(line);
        }

        return true;
    }

    #endregion

    #region Logging functions
    #region Static
    public static void Debug(string text, LogArea logArea)
    {
        #if DEBUG
        Instance.Log(text, logArea.ToString(), LogLevel.Debug);
        #endif
    }

    public static void Success(string text, LogArea logArea)
    {
        Instance.Log(text, logArea.ToString(), LogLevel.Success);
    }

    public static void Info(string text, LogArea logArea)
    {
        Instance.Log(text, logArea.ToString(), LogLevel.Info);
    }

    public static void Warn(string text, LogArea logArea)
    {
        Instance.Log(text, logArea.ToString(), LogLevel.Warning);
    }

    public static void Error(string text, LogArea logArea)
    {
        Instance.Log(text, logArea.ToString(), LogLevel.Error);
    }
    
    #endregion
    #region Instance-based
    public void LogDebug(string text, LogArea logArea)
    {
        #if DEBUG
        this.Log(text, logArea.ToString(), LogLevel.Debug);
        #endif
    }

    public void LogSuccess(string text, LogArea logArea)
    {
        this.Log(text, logArea.ToString(), LogLevel.Success);
    }

    public void LogInfo(string text, LogArea logArea)
    {
        this.Log(text, logArea.ToString(), LogLevel.Info);
    }

    public void LogWarn(string text, LogArea logArea)
    {
        this.Log(text, logArea.ToString(), LogLevel.Warning);
    }

    public void LogError(string text, LogArea logArea)
    {
        this.Log(text, logArea.ToString(), LogLevel.Error);
    }
    #endregion

    public void Log(string text, string area, LogLevel level, int extraTraceLines = 0)
    {
        this.queueLog(new LogLine
        {
            Level = level,
            Message = text,
            Area = area,
            Trace = getTrace(extraTraceLines),
        });
    }
    #endregion
}