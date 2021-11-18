using System.IO;
using System.Linq;

namespace LBPUnion.ProjectLighthouse.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string ToFileName(this string text) => Path.GetInvalidFileNameChars().Aggregate(text, (current, c) => current.Replace(c.ToString(), ""));

        public static string ToSafeXml(this string text) => text.Replace("<", "&lt;").Replace(">", "&gt;");
    }
}