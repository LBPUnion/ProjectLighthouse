using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Types.Files
{
    public class LbpFile
    {

        /// <summary>
        ///     A buffer of the file's data.
        /// </summary>
        public readonly byte[] Data;

        /// <summary>
        ///     The type of file.
        /// </summary>
        public LbpFileType FileType;

        public LbpFile(byte[] data)
        {
            this.Data = data;
            this.FileType = FileHelper.DetermineFileType(this.Data);
        }
    }
}