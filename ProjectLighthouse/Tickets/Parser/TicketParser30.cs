namespace LBPUnion.ProjectLighthouse.Tickets.Parser;

public class TicketParser30 : ITicketParser
{
    private readonly NPTicket ticket;
    private readonly TicketReader reader;

    public TicketParser30(NPTicket ticket, TicketReader reader)
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

        this.reader.ReadSectionHeader(); // date of birth section
        this.reader.ReadBytes(4); // 4 bytes for year month and day
        this.reader.ReadTicketUInt32();

        this.reader.ReadSectionHeader(); // empty section?
        this.reader.ReadTicketEmpty();

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