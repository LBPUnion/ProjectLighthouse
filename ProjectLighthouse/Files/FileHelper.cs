#nullable enable
using System;
using System.IO;
using System.Linq;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types.Resources;

namespace LBPUnion.ProjectLighthouse.Files;

public static partial class FileHelper
{
    public static readonly string ResourcePath = Path.Combine(Environment.CurrentDirectory, "r");

    public static readonly string FullResourcePath = Path.GetFullPath(ResourcePath);

    public static readonly string ImagePath = Path.Combine(Environment.CurrentDirectory, "png");

    public static readonly string FullImagePath = Path.GetFullPath(ImagePath);

    public static string GetResourcePath(string hash) => Path.Combine(ResourcePath, hash);

    public static string GetImagePath(string hash) => Path.Combine(ImagePath, hash);

    public static bool IsFileSafe(LbpFile file)
    {
        if (!ServerConfiguration.Instance.CheckForUnsafeFiles) return true;

        if (file.FileType == LbpFileType.Unknown) return false;

        return file.FileType switch
        {
            LbpFileType.MotionRecording => true,
            LbpFileType.StreamingChunk => true,
            LbpFileType.FileArchive => false,
            LbpFileType.CrossLevel => true,
            LbpFileType.Painting => true,
            LbpFileType.Unknown => false,
            LbpFileType.Texture => true,
            LbpFileType.Script => false,
            LbpFileType.Level => true,
            LbpFileType.Adventure => true,
            LbpFileType.Voice => true,
            LbpFileType.Quest => true,
            LbpFileType.Plan => true,
            LbpFileType.Jpeg => true,
            LbpFileType.Png => true,
            #if DEBUG
            _ => throw new ArgumentOutOfRangeException(nameof(file), @$"Unhandled file type ({file.FileType}) in FileHelper.IsFileSafe()"),
            #else
            _ => false,
            #endif
        };
    }

    public static bool ResourceExists(string hash) => File.Exists(GetResourcePath(hash));
    public static bool ImageExists(string hash) => File.Exists(GetImagePath(hash));

    public static void DeleteResource(string hash)
    {
        // Prevent directory traversal attacks
        if (!Path.GetFullPath(GetResourcePath(hash)).StartsWith(FullResourcePath)) return;

        // sanity check so someone doesn't somehow delete the entire resource folder
        if (ResourceExists(hash) && (File.GetAttributes(GetResourcePath(hash)) & FileAttributes.Directory) != FileAttributes.Directory)
        {
            File.Delete(GetResourcePath(hash));
        }

        string imageName = $"{hash}.png";
        if (!Path.GetFullPath(GetImagePath(imageName)).StartsWith(FullImagePath)) return;

        if (ImageExists(imageName) && (File.GetAttributes(GetImagePath(imageName)) & FileAttributes.Directory) != FileAttributes.Directory)
        {
            File.Delete(GetImagePath(imageName));
        }
    }

    public static int ResourceSize(string hash)
    {
        try
        {
            return (int)new FileInfo(GetResourcePath(hash)).Length;
        }
        catch
        {
            return 0;
        }
    }

    public static void EnsureDirectoryCreated(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path ?? throw new ArgumentNullException(nameof(path)));
    }

    public static string[] ResourcesNotUploaded(params string[] hashes) => hashes.Where(hash => !ResourceExists(hash)).ToArray();
}