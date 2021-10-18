using System;
using System.IO;
using System.Text;
using ProjectLighthouse.Types.Files;

namespace ProjectLighthouse.Helpers {
    public static class FileHelper {
        public static bool IsFileSafe(LbpFile file) {
            if(file.FileType == LbpFileType.Unknown) file.FileType = DetermineFileType(file.Data);
            
            return file.FileType switch {
                LbpFileType.Texture => true,
                LbpFileType.Script => false,
                LbpFileType.Level => true,
                LbpFileType.FileArchive => false,
                LbpFileType.Unknown => false,
                #if DEBUG
                _ => throw new ArgumentOutOfRangeException(nameof(file), $"Unhandled file type ({file.FileType}) in FileHelper.IsFileSafe()"),
                #else
                _ => false,
                #endif
            };
        }

        public static LbpFileType DetermineFileType(byte[] data) {
            using MemoryStream ms = new(data);
            using BinaryReader reader = new(ms);

            string footer = Encoding.ASCII.GetString(BinaryHelper.ReadLastBytes(reader, 4));
            if(footer == "FARC") return LbpFileType.FileArchive;

            byte[] header = reader.ReadBytes(3);

            return Encoding.ASCII.GetString(header) switch {
                "TEX" => LbpFileType.Texture,
                "FSH" => LbpFileType.Script,
                "LVL" => LbpFileType.Level,
                _ => LbpFileType.Unknown,
            };
        }
    }
}