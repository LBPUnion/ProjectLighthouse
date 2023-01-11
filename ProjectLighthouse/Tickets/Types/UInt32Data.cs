using System.Buffers.Binary;
using System.IO;

namespace LBPUnion.ProjectLighthouse.Tickets.Types;

public class UInt32Data : TicketData
{
    private readonly uint val;

    public UInt32Data(uint val)
    {
        this.val = val;
    }

    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        byte[] data = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(data, this.val);
        writer.Write(data);
    }

    public override short Id() => 1;

    public override short Len() => 4;
}