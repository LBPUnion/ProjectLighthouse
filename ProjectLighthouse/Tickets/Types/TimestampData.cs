using System.Buffers.Binary;
using System.IO;

namespace LBPUnion.ProjectLighthouse.Tickets.Types;

public class TimestampData : TicketData
{
    private readonly ulong val;

    public TimestampData(ulong val)
    {
        this.val = val;
    }

    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        byte[] data = new byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(data, this.val);
        writer.Write(data);
    }

    public override short Id() => 7;

    public override short Len() => 8;
}