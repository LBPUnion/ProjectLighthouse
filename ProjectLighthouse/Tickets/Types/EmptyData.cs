using System.IO;

namespace LBPUnion.ProjectLighthouse.Tickets.Types;

public class EmptyData : TicketData
{
    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
    }

    public override short Id() => 0;

    public override short Len() => 0;
}