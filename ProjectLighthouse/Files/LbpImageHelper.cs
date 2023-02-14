#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip.Compression;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Resources;
using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LBPUnion.ProjectLighthouse.Files;

public partial class FileHelper
{

    [GeneratedRegex(@"data:([^\/]+)\/([^;]+);base64,(.*)")]
    private static partial Regex ValidBase64Regex();

    private static byte[]? TryParseBase64Data(string b64)
    {
        Span<byte> buffer = new(new byte[b64.Length]);
        bool valid = Convert.TryFromBase64String(b64, buffer, out int bytesWritten);
        return valid ? buffer[..bytesWritten].ToArray() : null;
    }

    public static async Task<string?> ParseBase64Image(string? image)
    {
        if (string.IsNullOrWhiteSpace(image)) return null;

        Match match = ValidBase64Regex().Match(image);

        if (!match.Success) return null;

        if (match.Groups.Count != 4) return null;

        byte[]? data = TryParseBase64Data(match.Groups[3].Value);
        if (data == null) return null;

        LbpFile file = new(data);

        if (file.FileType is not (LbpFileType.Jpeg or LbpFileType.Png)) return null;

        if (ResourceExists(file.Hash)) return file.Hash;

        string assetsDirectory = ResourcePath;
        string path = GetResourcePath(file.Hash);

        EnsureDirectoryCreated(assetsDirectory);
        await File.WriteAllBytesAsync(path, file.Data);
        return file.Hash;
    }

    public static void ConvertAllTexturesToPng()
    {
        EnsureDirectoryCreated(Path.Combine(Environment.CurrentDirectory, "png"));
        if (!Directory.Exists("r")) return;

        Logger.Info(
            "Converting all textures to PNG. This may take a while if this is the first time running this operation...",
            LogArea.Startup);

        ConcurrentQueue<string> fileQueue = new();

        foreach (string filename in Directory.GetFiles("r")) fileQueue.Enqueue(filename);

        List<Task> taskList = new();

        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            taskList.Add(Task.Factory.StartNew(() =>
            {
                while (fileQueue.TryDequeue(out string? filename))
                {
                    LbpFile? file = LbpFile.FromHash(filename.Replace("r" + Path.DirectorySeparatorChar, ""));

                    if (file?.FileType is LbpFileType.Jpeg or LbpFileType.Png or LbpFileType.Texture)
                    {
                        LbpFileToPNG(file);
                    }
                }
            }));
        }

        Task.WaitAll(taskList.ToArray());
    }

    public static bool LbpFileToPNG(LbpFile file) => LbpFileToPNG(file.Data, file.Hash, file.FileType);

    private static bool LbpFileToPNG(byte[] data, string hash, LbpFileType type)
    {
        if (type != LbpFileType.Jpeg && type != LbpFileType.Png && type != LbpFileType.Texture) return false;

        if (File.Exists(Path.Combine("png", $"{hash}.png"))) return true;

        using MemoryStream ms = new(data);
        using BinaryReader reader = new(ms);

        try
        {
            return type switch
            {
                LbpFileType.Texture => TextureToPNG(hash, reader),
                LbpFileType.Png => PNGToPNG(hash, data),
                LbpFileType.Jpeg => JPGToPNG(hash, data),
                // ReSharper disable once UnreachableSwitchArmDueToIntegerAnalysis
                _ => false,
            };
        }
        catch (Exception e)
        {
            Logger.Error($"Error while converting {type} {hash}: \n{e}", LogArea.Resources);
            return false;
        }
    }

    private static bool TextureToPNG(string hash, BinaryReader reader)
    {
        // Skip the magic (3 bytes), we already know its a texture
        for (int i = 0; i < 3; i++) reader.ReadByte();

        // This below is shamelessly stolen from ennuo's Toolkit: https://github.com/ennuo/toolkit/blob/d996ee4134740db0ee94e2cbf1e4edbd1b5ec798/src/main/java/ennuo/craftworld/utilities/Compressor.java#L40

        // This byte determines the method of reading. We can only read a texture (' ') so if it's not ' ' it must be invalid.
        if ((char)reader.ReadByte() != ' ') return false;

        reader.ReadInt16(); // ?
        short chunks = reader.ReadInt16BE();

        int[] compressed = new int[chunks];
        int[] decompressed = new int[chunks];

        for (int i = 0; i < chunks; ++i)
        {
            compressed[i] = reader.ReadUInt16BE();
            decompressed[i] = reader.ReadUInt16BE();
        }

        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        for (int i = 0; i < chunks; ++i)
        {
            byte[] deflatedData = reader.ReadBytes(compressed[i]);
            if (compressed[i] == decompressed[i])
            {
                writer.Write(deflatedData);
                continue;
            }

            Inflater inflater = new();
            inflater.SetInput(deflatedData);
            byte[] inflatedData = new byte[decompressed[i]];
            inflater.Inflate(inflatedData);

            writer.Write(inflatedData);
        }

        return DDSToPNG(hash, ms.ToArray());
    }

    private static bool DDSToPNG(string hash, byte[] data)
    {
        Dds ddsImage = Dds.Create(data, new PfimConfig());
        if (ddsImage.Compressed) ddsImage.Decompress();

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        Image image = ddsImage.Format switch
        {
            ImageFormat.Rgba32 => Image.LoadPixelData<Bgra32>(ddsImage.Data, ddsImage.Width, ddsImage.Height),
            _ => throw new ArgumentOutOfRangeException($"ddsImage.Format is not supported: {ddsImage.Format}"),
        };

        Directory.CreateDirectory("png");
        image.SaveAsPngAsync($"png/{hash}.png");
        return true;
    }

    private static bool JPGToPNG(string hash, byte[] data)
    {
        using Image image = Image.Load(data);
        using MemoryStream ms = new();
        image.SaveAsPng(ms);

        File.WriteAllBytes($"png/{hash}.png", ms.ToArray());
        return true;
    }

    // it sounds dumb i know but hear me out:
    // you're completely correct
    private static bool PNGToPNG(string hash, byte[] data)
    {
        File.WriteAllBytes($"png/{hash}.png", data);
        return true;
    }
}