using System.IO;

namespace LBPUnion.ProjectLighthouse.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string ToFileName(this string text)
        {
            char[] invalidPathChars = Path.GetInvalidFileNameChars();
            string path = text;
            
            foreach (char c in invalidPathChars)
            {
                path = path.Replace(c.ToString(), "");
            }

            return path;
        }
    }
}