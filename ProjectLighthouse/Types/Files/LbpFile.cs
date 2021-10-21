using System.IO;
using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Types.Files {
    public class LbpFile {
        public LbpFile(byte[] data) {
            this.Data = data;
            this.FileType = FileHelper.DetermineFileType(this.Data);
        }

        public LbpFile(Stream stream) {
            using MemoryStream ms = new();
            stream.CopyToAsync(ms);

            this.Data = ms.ToArray();
            this.FileType = FileHelper.DetermineFileType(this.Data);
        }
        
        /// <summary>
        /// The type of file.
        /// </summary>
        public LbpFileType FileType;

        /// <summary>
        /// A buffer of the file's data.
        /// </summary>
        public readonly byte[] Data;

    }
}