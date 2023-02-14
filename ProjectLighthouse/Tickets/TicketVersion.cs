namespace LBPUnion.ProjectLighthouse.Tickets;

public struct TicketVersion
{
    public byte Major { get; set; }
    public byte Minor { get; set; }

    public TicketVersion(byte major, byte minor)
    {
        this.Major = major;
        this.Minor = minor;
    }

    public override string ToString() => $"{this.Major}.{this.Minor}";

    public static implicit operator string(TicketVersion v) => v.ToString();
}