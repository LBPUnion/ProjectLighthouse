using System.IO;
using System.Text;

namespace LBPUnion.ProjectLighthouse.Tickets.Types;

public class StringData : TicketData
{
    private readonly byte[] val;

    public StringData(string val)
    {
        this.val = Encoding.ASCII.GetBytes(val);
    }

    public StringData(byte[] val)
    {
        this.val = val;
    }

    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        writer.Write(this.val);
    }

    public override short Id() => 4;

    public override short Len() => (short)this.val.Length;
}