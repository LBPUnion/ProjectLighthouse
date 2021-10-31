using System;
using System.IO;
using System.Linq;
using System.Text;
using LBPUnion.ProjectLighthouse.Types.Files;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    public static class FileHelper
    {
        public static readonly string ResourcePath = Path.Combine(Environment.CurrentDirectory, "r");

        public static string GetResourcePath(string hash) => Path.Combine(ResourcePath, hash);

        public static bool IsFileSafe(LbpFile file)
        {
            if (file.FileType == LbpFileType.Unknown) file.FileType = DetermineFileType(file.Data);

            return file.FileType switch
            {
                LbpFileType.FileArchive => false,
                LbpFileType.Painting => true,
                LbpFileType.Unknown => false,
                LbpFileType.Texture => true,
                LbpFileType.Script => false,
                LbpFileType.Level => true,
                LbpFileType.Voice => true,
                LbpFileType.Plan => true,
                #if DEBUG
                _ => throw new ArgumentOutOfRangeException(nameof(file), $"Unhandled file type ({file.FileType}) in FileHelper.IsFileSafe()"),
                #else
                _ => false,
                #endif
            };
        }

        public static LbpFileType DetermineFileType(byte[] data)
        {
            using MemoryStream ms = new(data);
            using BinaryReader reader = new(ms);

            string footer = Encoding.ASCII.GetString(BinaryHelper.ReadLastBytes(reader, 4));
            if (footer == "FARC") return LbpFileType.FileArchive;

            byte[] header = reader.ReadBytes(3);

            return Encoding.ASCII.GetString(header) switch
            {
                "PTG" => LbpFileType.Painting,
                "TEX" => LbpFileType.Texture,
                "FSH" => LbpFileType.Script,
                "VOP" => LbpFileType.Voice,
                "LVL" => LbpFileType.Level,
                "PLN" => LbpFileType.Plan,
                _ => LbpFileType.Unknown,
            };
        }

        public static bool ResourceExists(string hash) => File.Exists(GetResourcePath(hash));

        public static void EnsureDirectoryCreated(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path ?? throw new ArgumentNullException(nameof(path)));
        }

        public static string[] ResourcesNotUploaded(params string[] hashes) => hashes.Where(hash => !ResourceExists(hash)).ToArray();
    }
}