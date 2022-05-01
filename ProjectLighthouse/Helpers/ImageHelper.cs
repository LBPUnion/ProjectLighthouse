#nullable enable
using System;
using System.IO;
using DDSReader;
using ICSharpCode.SharpZipLib.Zip.Compression;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;
using LBPUnion.ProjectLighthouse.Types.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class ImageHelper
{
    public static bool LbpFileToPNG(LbpFile file) => LbpFileToPNG(file.Data, file.Hash, file.FileType);

    public static bool LbpFileToPNG(byte[] data, string hash, LbpFileType type)
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
        catch(Exception e)
        {
            Console.WriteLine($"Error while converting {hash}:");
            Console.WriteLine(e);
            return false;
        }
    }

    private static bool TextureToPNG(string hash, BinaryReader reader)
    {
        // Skip the magic (3 bytes), we already know its a texture
        for(int i = 0; i < 3; i++) reader.ReadByte();

        // This below is shamelessly stolen from ennuo's Toolkit: https://github.com/ennuo/toolkit/blob/d996ee4134740db0ee94e2cbf1e4edbd1b5ec798/src/main/java/ennuo/craftworld/utilities/Compressor.java#L40

        // This byte determines the method of reading. We can only read a texture (' ') so if it's not ' ' it must be invalid.
        if ((char)reader.ReadByte() != ' ') return false;

        reader.ReadInt16(); // ?
        short chunks = reader.ReadInt16BE();

        int[] compressed = new int[chunks];
        int[] decompressed = new int[chunks];

        for(int i = 0; i < chunks; ++i)
        {
            compressed[i] = reader.ReadUInt16BE();
            decompressed[i] = reader.ReadUInt16BE();
        }

        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        for(int i = 0; i < chunks; ++i)
        {
            byte[] deflatedData = reader.ReadBytes(compressed[i]);
            if (compressed[i] == decompressed[i])
            {
                writer.Write(deflatedData);
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
        using MemoryStream stream = new();
        DDSImage image = new(data);

        image.SaveAsPng(stream);

        Directory.CreateDirectory("png");
        File.WriteAllBytes($"png/{hash}.png", stream.ToArray());
        return true;
    }

    private static bool JPGToPNG(string hash, byte[] data)
    {
        using Image<Rgba32> image = Image.Load(data);
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