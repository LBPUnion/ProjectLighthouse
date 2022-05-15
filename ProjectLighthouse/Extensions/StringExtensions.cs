using System.IO;
using System.Linq;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class StringExtensions
{
    public static string ToFileName(this string text) => Path.GetInvalidFileNameChars().Aggregate(text, (current, c) => current.Replace(c.ToString(), ""));
}