using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LBPUnion.ProjectLighthouse.Tickets.Types;

public class BlobData : TicketData
{
    private readonly byte id;
    private readonly List<TicketData> data;

    public BlobData(byte id, List<TicketData> data)
    {
        this.id = id;
        this.data = data;
    }
    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        foreach (TicketData d in this.data)
        {
            d.Write(writer);
        }
    }

    public override short Id() => (short)(0x3000 | this.id);

    public override short Len() => (short)this.data.Sum(d => d.Len()+4);
}