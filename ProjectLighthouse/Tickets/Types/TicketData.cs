using System.Buffers.Binary;
using System.IO;

namespace LBPUnion.ProjectLighthouse.Tickets.Types;

public abstract class TicketData
{
    public void WriteHeader(BinaryWriter writer)
    {
        byte[] id = new byte[2];
        byte[] len = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(id, (ushort)this.Id());
        BinaryPrimitives.WriteUInt16BigEndian(len, (ushort)this.Len());
        writer.Write(id);
        writer.Write(len);
    }

    public abstract void Write(BinaryWriter writer);
    public abstract short Id();
    public abstract short Len();
}