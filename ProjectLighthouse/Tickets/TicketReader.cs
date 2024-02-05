using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Extensions;

namespace LBPUnion.ProjectLighthouse.Tickets;

public enum DataType : byte
{
    Empty = 0x00,
    UInt32 = 0x01,
    UInt64 = 0x02,
    String = 0x04,
    Timestamp = 0x07,
    Binary = 0x08,
}

public enum SectionType : byte
{
    Body = 0x00,
    Footer = 0x02,
}

public struct DataHeader
{
    public DataType Type;
    public ushort Length;
}

public struct SectionHeader
{
    public SectionType Type;
    public ushort Length;
    public int Position;
}

public class TicketReader : BinaryReader
{
    public TicketReader([NotNull] Stream input) : base(input)
    {}

    public TicketVersion ReadTicketVersion() => (TicketVersion)this.ReadUInt16BE();

    public void SkipBytes(long bytes) => this.BaseStream.Position += bytes;

    public SectionHeader ReadSectionHeader()
    {
        this.ReadByte();

        SectionHeader sectionHeader = new()
        {
            Type = (SectionType)this.ReadByte(),
            Length = this.ReadUInt16BE(),
            Position = (int)(this.BaseStream.Position - 4),
        };

        return sectionHeader;
    }

    private DataHeader ReadDataHeader()
    {
        DataHeader dataHeader = new()
        {
            Type = (DataType)this.ReadUInt16BE(),
            Length = this.ReadUInt16BE(),
        };

        return dataHeader;
    }

    public byte[] ReadTicketBinary()
    {
        DataHeader dataHeader = this.ReadDataHeader();
        Debug.Assert(dataHeader.Type is DataType.Binary or DataType.String);

        return this.ReadBytes(dataHeader.Length);
    }

    public string ReadTicketString() => Encoding.UTF8.GetString(this.ReadTicketBinary()).TrimEnd('\0');

    public uint ReadTicketUInt32()
    {
        DataHeader dataHeader = this.ReadDataHeader();
        Debug.Assert(dataHeader.Type == DataType.UInt32);

        return this.ReadUInt32BE();
    }

    public void ReadTicketEmpty()
    {
        DataHeader dataHeader = this.ReadDataHeader();
        Debug.Assert(dataHeader.Type == DataType.Empty);
    }

    public ulong ReadTicketUInt64()
    {
        DataHeader dataHeader = this.ReadDataHeader();
        Debug.Assert(dataHeader.Type is DataType.UInt64 or DataType.Timestamp);

        return this.ReadUInt64BE();
    }
}