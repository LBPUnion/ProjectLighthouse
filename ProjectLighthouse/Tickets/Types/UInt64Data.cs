using System.Buffers.Binary;
using System.IO;

namespace LBPUnion.ProjectLighthouse.Tickets.Types;

public class UInt64Data : TicketData
{
    private readonly ulong val;

    public UInt64Data(ulong val)
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

    public override short Id() => 2;

    public override short Len() => 8;
}