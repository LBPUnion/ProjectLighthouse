using System;

namespace LBPUnion.ProjectLighthouse.Helpers.Extensions;

internal static class ConsoleColorExtensions
{
    internal static ConsoleColor ToDark(this ConsoleColor color)
        => color switch
        {
            ConsoleColor.Blue => ConsoleColor.DarkBlue,
            ConsoleColor.Cyan => ConsoleColor.DarkCyan,
            ConsoleColor.Green => ConsoleColor.DarkGreen,
            ConsoleColor.Gray => ConsoleColor.DarkGray,
            ConsoleColor.Magenta => ConsoleColor.DarkMagenta,
            ConsoleColor.Red => ConsoleColor.DarkRed,
            ConsoleColor.White => ConsoleColor.Gray,
            ConsoleColor.Yellow => ConsoleColor.DarkYellow,
            _ => color,
        };
}