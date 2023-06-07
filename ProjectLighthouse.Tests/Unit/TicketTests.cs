using LBPUnion.ProjectLighthouse.Tickets;
using LBPUnion.ProjectLighthouse.Tickets.Signature;
using LBPUnion.ProjectLighthouse.Types.Users;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Unit;

[Trait("Category", "Unit")]
public class TicketTests
{
    [Fact]
    public void CanReadTicket()
    {
        TicketBuilder builder = new TicketBuilder()
            .SetCountry("br")
            .SetUserId(21)
            .SetDomain("us")
            .SetStatus(0)
            .SetIssuerId(0x74657374)
            .setExpirationTime(ulong.MaxValue)
            .SetUsername("unittest")
            .SetIssueTime(0)
            .SetTitleId("UP9000-BCUS98245_00");

        byte[] ticketData = builder.Build();

        NPTicket? ticket = NPTicket.CreateFromBytes(ticketData);
        Assert.NotNull(ticket);
        Assert.Equal((ulong)0, ticket.IssueTime);
        Assert.Equal(ulong.MaxValue, ticket.ExpireTime);
        Assert.Equal("unittest", ticket.Username);
        Assert.Equal(GameVersion.LittleBigPlanet2, ticket.GameVersion);
        Assert.Equal((ulong)0x74657374, ticket.IssuerId);
        Assert.Equal((ulong)21, ticket.UserId);
    }

    [Fact]
    public void CanVerifyTicketSignature()
    {
        TicketBuilder builder = new();

        byte[] ticketData = builder.Build();

        NPTicket? ticket = NPTicket.CreateFromBytes(ticketData);
        Assert.NotNull(ticket);
        Assert.True(new UnitTestSignatureVerifier(ticket).ValidateSignature());
    }
}