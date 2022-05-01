using System;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Helpers.Extensions;

public static class LogLevelExtensions
{
    public static ConsoleColor ToColor(this LogLevel level)
        => level switch
        {
            LogLevel.Debug => ConsoleColor.Magenta,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Success => ConsoleColor.Green,
            _ => ConsoleColor.White,
        };
}