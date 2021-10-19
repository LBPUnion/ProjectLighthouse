using ProjectLighthouse.Helpers;

namespace ProjectLighthouse.Types.Files {
    public class LbpFile {
        public LbpFile(byte[] data) {
            this.Data = data;
            this.FileType = FileHelper.DetermineFileType(data);
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