using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Tickets.Data;
using LBPUnion.ProjectLighthouse.Types;

namespace LBPUnion.ProjectLighthouse.Tickets;

public class TicketReader : BinaryReader
{
    public TicketReader([NotNull] Stream input) : base(input)
    {}

    public Version ReadTicketVersion() => new((byte)(this.ReadByte() >> 4), this.ReadByte());

    public SectionHeader ReadSectionHeader()
    {
        this.ReadByte();

        SectionHeader sectionHeader = new()
        {
            Type = (SectionType)this.ReadByte(),
            Length = this.ReadUInt16BE(),
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