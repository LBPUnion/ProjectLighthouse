using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;

namespace LBPUnion.ProjectLighthouse.Types.Tickets;

public class TicketReader : BinaryReader
{
    public TicketReader([NotNull] Stream input) : base(input)
    {}

    public Version ReadTicketVersion() => new(this.ReadByte() >> 4, this.ReadByte());

    public SectionHeader ReadSectionHeader()
    {
        this.ReadByte();

        SectionHeader sectionHeader = new();
        sectionHeader.Type = (SectionType)this.ReadByte();
        sectionHeader.Length = this.ReadUInt16BE();

        return sectionHeader;
    }

    public DataHeader ReadDataHeader()
    {
        DataHeader dataHeader = new();
        dataHeader.Type = (DataType)this.ReadUInt16BE();
        dataHeader.Length = this.ReadUInt16BE();

        return dataHeader;
    }

    public byte[] ReadTicketBinary()
    {
        DataHeader dataHeader = this.ReadDataHeader();
        Debug.Assert(dataHeader.Type == DataType.Binary || dataHeader.Type == DataType.String);

        return this.ReadBytes(dataHeader.Length);
    }

    public string ReadTicketString() => Encoding.UTF8.GetString(this.ReadTicketBinary()).TrimEnd('\0');

    public uint ReadTicketUInt32()
    {
        DataHeader dataHeader = this.ReadDataHeader();
        Debug.Assert(dataHeader.Type == DataType.UInt32);

        return this.ReadUInt32BE();
    }

    public ulong ReadTicketUInt64()
    {
        DataHeader dataHeader = this.ReadDataHeader();
        Debug.Assert(dataHeader.Type == DataType.UInt64 || dataHeader.Type == DataType.Timestamp);

        return this.ReadUInt64BE();
    }
}