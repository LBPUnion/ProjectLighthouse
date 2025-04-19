namespace LBPUnion.ProjectLighthouse.Tickets.Parser;

public class TicketParser21 : ITicketParser
{
    private readonly NPTicket ticket;
    private readonly TicketReader reader;

    public TicketParser21(NPTicket ticket, TicketReader reader)
    {
        this.ticket = ticket;
        this.reader = reader;
    }
    public bool ParseTicket()
    {
        this.reader.ReadTicketString(); // serial id

        this.ticket.IssuerId = this.reader.ReadTicketUInt32();
        this.ticket.IssueTime = this.reader.ReadTicketUInt64();
        this.ticket.ExpireTime = this.reader.ReadTicketUInt64();

        this.ticket.UserId = this.reader.ReadTicketUInt64();

        this.ticket.Username = this.reader.ReadTicketString();

        this.reader.ReadTicketString(); // Country
        this.reader.ReadTicketString(); // Domain

        this.ticket.TitleId = this.reader.ReadTicketString();

        this.reader.ReadTicketUInt32(); // status

        SectionHeader entitlementsSection = this.reader.ReadSectionHeader(); // entitlements section
        this.reader.SkipBytes(entitlementsSection.Length);

        this.reader.ReadTicketEmpty(); // padding

        SectionHeader footer = this.reader.ReadSectionHeader(); // footer header
        if (footer.Type != SectionType.Footer)
        {
            throw new TicketParseException(@$"Unexpected ticket footer header: expected={SectionType.Footer}, actual={footer}");
        }

        this.ticket.TicketSignatureIdentifier = this.reader.ReadTicketBinary();

        this.ticket.TicketSignature = this.reader.ReadTicketBinary();
        return true;
    }
}