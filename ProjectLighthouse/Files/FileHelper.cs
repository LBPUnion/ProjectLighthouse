#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip.Compression;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LBPUnion.ProjectLighthouse.Files;

public static class FileHelper
{
    public static readonly string ResourcePath = Path.Combine(Environment.CurrentDirectory, "r");

    public static string GetResourcePath(string hash) => Path.Combine(ResourcePath, hash);

    public static bool AreDependenciesSafe(LbpFile file)
    {
        // recursively check if dependencies are safe
        List<ResourceDescriptor> dependencies = ParseDependencyList(file);
        foreach (ResourceDescriptor resource in dependencies)
        {
            if (resource.IsGuidResource()) continue;

            LbpFile? r = LbpFile.FromHash(resource.Hash);
            // If the resource hasn't been uploaded yet then we just go off it's included resource type
            if (r == null)
                if (resource.IsScriptType())
                    return false;
                else
                    continue;

            if (!IsFileSafe(r)) return false;
        }

        return true;
    }

    public static bool IsFileSafe(LbpFile file)
    {
        if (!ServerConfiguration.Instance.CheckForUnsafeFiles) return true;

        if (file.FileType == LbpFileType.Unknown) return false;

        return file.FileType switch
        {
            LbpFileType.MotionRecording => true,
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
            _ => throw new ArgumentOutOfRangeException(nameof(file), $"Unhandled file type ({file.FileType}) in FileHelper.IsFileSafe()"),
            #else
            _ => false,
            #endif
        };
    }

    private static List<ResourceDescriptor> ParseDependencyList(LbpFile file)
    {

        List<ResourceDescriptor> dependencies = new();
        if (file.FileType == LbpFileType.Unknown || file.Data.Length < 0xb || file.Data[3] != 'b')
        {
            return dependencies;
        }

        int revision = BinaryPrimitives.ReadInt32BigEndian(file.Data.AsSpan()[4..]);

        // Data format is 'borrowed' from: https://github.com/ennuo/toolkit/blob/main/src/main/java/ennuo/craftworld/resources/Resource.java#L191

        if (revision < 0x109) return dependencies;

        int curOffset = 8;
        int dependencyTableOffset = BinaryPrimitives.ReadInt32BigEndian(file.Data.AsSpan()[curOffset..]);
        if(dependencyTableOffset <= 0 || dependencyTableOffset > file.Data.Length) return dependencies;
        
        curOffset = dependencyTableOffset;
        int dependencyTableSize = BinaryPrimitives.ReadInt32BigEndian(file.Data.AsSpan()[dependencyTableOffset..]);
        curOffset += 4;
        for (int i = 0; i < dependencyTableSize; ++i)
        {
            byte hashType = file.Data[curOffset];
            curOffset += 1;
            ResourceDescriptor resource = new();
            switch (hashType)
            {
                case 1:
                {
                    byte[] hashBytes = new byte[0x14];
                    Buffer.BlockCopy(file.Data, curOffset, hashBytes, 0, 0x14);
                    curOffset += 0x14;
                    resource.Hash = BitConverter.ToString(hashBytes).Replace("-", "");
                    break;
                }
                case 2:
                {
                    resource.Hash = "g" + BinaryPrimitives.ReadUInt32BigEndian(file.Data.AsSpan()[curOffset..]);
                    curOffset += 4;
                    break;
                }
            }
            resource.Type = BinaryPrimitives.ReadInt32BigEndian(file.Data.AsSpan()[curOffset..]);
            curOffset += 4;
            dependencies.Add(resource);
        }
        return dependencies;
    }

    public static GameVersion ParseLevelVersion(LbpFile file)
    {
        if (file.FileType != LbpFileType.Level || file.Data.Length < 16 || file.Data[3] != 'b') return GameVersion.Unknown;

        // Revision numbers borrowed from https://github.com/ennuo/toolkit/blob/main/src/main/java/ennuo/craftworld/resources/structs/Revision.java

        const ushort lbp2Latest = 0x3F8;
        const ushort lbp1Latest = 0x272;
        const ushort lbpVitaLatest = 0x3E2;
        const ushort lbpVitaDescriptor = 0x4431;
        // There are like 1600 revisions so this doesn't cover everything

        int revision = BinaryPrimitives.ReadInt32BigEndian(file.Data.AsSpan()[4..]);

        if (revision >= 0x271)
        {
            // construct a 16 bit number from 2 individual bytes
            ushort branchDescriptor = BinaryPrimitives.ReadUInt16BigEndian(file.Data.AsSpan()[12..]);
            if (revision == lbpVitaLatest && branchDescriptor == lbpVitaDescriptor) return GameVersion.LittleBigPlanetVita;
        }

        GameVersion version = GameVersion.Unknown;
        if (revision <= lbp1Latest)
        {
            version = GameVersion.LittleBigPlanet1;
        }
        else if (revision >> 0x10 != 0)
        {
            version = GameVersion.LittleBigPlanet3;
        }
        else if(revision <= lbp2Latest)
        {
            version = GameVersion.LittleBigPlanet2;
        }

        return version;
    }

    public static LbpFileType DetermineFileType(byte[] data)
    {
        if (data.Length == 0) return LbpFileType.Unknown; // Can't be anything if theres no data.

        using MemoryStream ms = new(data);
        using BinaryReader reader = new(ms);

        // Determine if file is a FARC (File Archive).
        // Needs to be done before anything else that determines the type by the header
        // because this determines the type by the footer.
        string footer = Encoding.ASCII.GetString(readLastBytes(reader, 4));
        if (footer == "FARC") return LbpFileType.FileArchive;

        byte[] header = reader.ReadBytes(4);

        return Encoding.ASCII.GetString(header) switch
        {
            "RECb" => LbpFileType.MotionRecording,
            "PRFb" => LbpFileType.CrossLevel,
            "PTGb" => LbpFileType.Painting,
            "TEX " => LbpFileType.Texture,
            "FSHb" => LbpFileType.Script,
            "VOPb" => LbpFileType.Voice,
            "LVLb" => LbpFileType.Level,
            "ADCb" => LbpFileType.Adventure,
            "ADSb" => LbpFileType.Adventure,
            "PLNb" => LbpFileType.Plan,
            "QSTb" => LbpFileType.Quest,
            _ => readAlternateHeader(reader),
        };
    }

    private static byte[] readLastBytes(BinaryReader reader, int count, bool restoreOldPosition = true)
    {
        long oldPosition = reader.BaseStream.Position;

        if (reader.BaseStream.Length < count) return Array.Empty<byte>();

        reader.BaseStream.Position = reader.BaseStream.Length - count;
        byte[] data = reader.ReadBytes(count);

        if (restoreOldPosition) reader.BaseStream.Position = oldPosition;
        return data;
    }

    private static LbpFileType readAlternateHeader(BinaryReader reader)
    {
        reader.BaseStream.Position = 0;

        // Determine if file is JPEG/PNG
        byte[] header = reader.ReadBytes(9);

        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF && header[3] == 0xE0) return LbpFileType.Jpeg;
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return LbpFileType.Png;

        return LbpFileType.Unknown; // Still unknown.
    }

    public static bool ResourceExists(string hash) => File.Exists(GetResourcePath(hash));

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

    private static readonly Regex base64Regex = new(@"data:([^\/]+)\/([^;]+);base64,(.*)", RegexOptions.Compiled);

    public static async Task<string?> ParseBase64Image(string? image)
    {
        if (string.IsNullOrWhiteSpace(image)) return null;

        System.Text.RegularExpressions.Match match = base64Regex.Match(image);

        if (!match.Success) return null;

        if (match.Groups.Count != 4) return null;

        byte[] data = Convert.FromBase64String(match.Groups[3].Value);

        LbpFile file = new(data);

        if (file.FileType is not (LbpFileType.Jpeg or LbpFileType.Png)) return null;

        if (ResourceExists(file.Hash)) return file.Hash;

        string assetsDirectory = ResourcePath;
        string path = GetResourcePath(file.Hash);

        EnsureDirectoryCreated(assetsDirectory);
        await File.WriteAllBytesAsync(path, file.Data);
        return file.Hash;
    }

    public static string[] ResourcesNotUploaded(params string[] hashes) => hashes.Where(hash => !ResourceExists(hash)).ToArray();

    public static void ConvertAllTexturesToPng()
    {
        EnsureDirectoryCreated(Path.Combine(Environment.CurrentDirectory, "png"));
        if (Directory.Exists("r"))
        {
            Logger.Info("Converting all textures to PNG. This may take a while if this is the first time running this operation...", LogArea.Startup);

            ConcurrentQueue<string> fileQueue = new();

            foreach (string filename in Directory.GetFiles("r")) fileQueue.Enqueue(filename);

            for(int i = 0; i < Environment.ProcessorCount; i++)
            {
                Task.Factory.StartNew
                (
                    () =>
                    {
                        while (fileQueue.TryDequeue(out string? filename))
                        {
                            LbpFile? file = LbpFile.FromHash(filename.Replace("r" + Path.DirectorySeparatorChar, ""));
                            if (file == null) continue;

                            if (file.FileType == LbpFileType.Jpeg || file.FileType == LbpFileType.Png || file.FileType == LbpFileType.Texture)
                            {
                                LbpFileToPNG(file);
                            }
                        }
                    }
                );
            }

            while (!fileQueue.IsEmpty)
            {
                Thread.Sleep(100);
            }
        }
    }

    #region Images

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
        if(ddsImage.Compressed)
            ddsImage.Decompress();

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        Image image = ddsImage.Format switch
        {
            ImageFormat.Rgba32 => Image.LoadPixelData<Bgra32>(ddsImage.Data, ddsImage.Width, ddsImage.Height),
            _ => throw new ArgumentOutOfRangeException($"ddsImage.Format is not supported: {ddsImage.Format}")
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

    #endregion

}
