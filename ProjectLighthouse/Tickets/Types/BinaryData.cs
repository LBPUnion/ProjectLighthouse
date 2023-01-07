using System.IO;

namespace LBPUnion.ProjectLighthouse.Tickets.Types;

public class BinaryData : TicketData
{
    private readonly byte[] val;

    public BinaryData(byte[] val)
    {
        this.val = val;
    }

    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        writer.Write(this.val);
    }

    public override short Id() => 8;

    public override short Len() => (short)this.val.Length;
}