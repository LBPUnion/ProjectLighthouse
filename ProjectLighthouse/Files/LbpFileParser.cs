#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LBPUnion.ProjectLighthouse.Types.Resources;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Files;

internal class ResourceDescriptor
{
    public string Hash = "";
    public int Type;

    public bool IsScriptType() => this.Type == 0x11;

    public bool IsGuidResource() => this.Hash.StartsWith("g");
}

public partial class FileHelper
{

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
            "CHKb" => LbpFileType.StreamingChunk,
            _ => readAlternateHeader(reader),
        };
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

    private static byte[] readLastBytes(BinaryReader reader, int count, bool restoreOldPosition = true)
    {
        long oldPosition = reader.BaseStream.Position;

        if (reader.BaseStream.Length < count) return Array.Empty<byte>();

        reader.BaseStream.Position = reader.BaseStream.Length - count;
        byte[] data = reader.ReadBytes(count);

        if (restoreOldPosition) reader.BaseStream.Position = oldPosition;
        return data;
    }

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
        if (dependencyTableOffset <= 0 || dependencyTableOffset > file.Data.Length) return dependencies;

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
        if (file.FileType != LbpFileType.Level || file.Data.Length < 16 || file.Data[3] != 'b')
            return GameVersion.Unknown;

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
            if (revision == lbpVitaLatest && branchDescriptor == lbpVitaDescriptor)
                return GameVersion.LittleBigPlanetVita;
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
        else if (revision <= lbp2Latest)
        {
            version = GameVersion.LittleBigPlanet2;
        }

        return version;
    }

}