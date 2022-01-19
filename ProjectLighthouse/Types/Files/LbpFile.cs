#nullable enable
using System.IO;
using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Types.Files;

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

    public static LbpFile? FromHash(string hash)
    {
        string path = FileHelper.GetResourcePath(hash);
        if (!File.Exists(path)) return null;

        byte[] data = File.ReadAllBytes(path);

        return new LbpFile(data);
    }
}