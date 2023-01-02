using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tickets.Types;

namespace LBPUnion.ProjectLighthouse.Tickets;

// generates v2.1 tickets
public class TicketBuilder
{
    private static uint _idDispenser;

    private string Username { get; set; } = "test";
    private string TitleId { get; set; } = "test";
    private ulong ExpirationTime { get; set; } = (ulong)TimeHelper.TimestampMillis;
    private ulong IssueTime { get; set; } = (ulong)TimeHelper.TimestampMillis + 15 * 60 * 1000;
    private uint IssuerId { get; set; } = 0x74657374;
    private ulong UserId { get; set; }
    private string Country { get; set; } = "br";
    private string Domain { get; set; } = "un";
    private uint Status { get; set; }

    public byte[] Build()
    {
        uint ticketId = _idDispenser++;
        byte[] serialBytes = new byte[0x14];
        BinaryPrimitives.WriteUInt32BigEndian(serialBytes, ticketId);
        // uint issuerId = 0x62727568;
        byte[] onlineId = new byte[0x20];
        Encoding.ASCII.GetBytes(this.Username).CopyTo(onlineId, 0);

        byte[] country = new byte[0x4];
        Encoding.ASCII.GetBytes(this.Country).CopyTo(country, 0);

        byte[] domain = new byte[0x4];
        Encoding.ASCII.GetBytes(this.Domain).CopyTo(domain, 0);

        byte[] titleId = new byte[0x18];
        Encoding.ASCII.GetBytes(this.TitleId).CopyTo(titleId, 0);

        List<TicketData> userData = new()
        {
            new BinaryData(serialBytes),
            new UInt32Data(this.IssuerId),
            new TimestampData(this.IssueTime),
            new TimestampData(this.ExpirationTime),
            new UInt64Data(this.UserId),
            new StringData(onlineId),
            new BinaryData(country),
            new BinaryData(domain),
            new BinaryData(titleId),
            new UInt32Data(this.Status),
            new EmptyData(),
            new EmptyData(),
        };
        TicketData userBlob = new BlobData(0, userData);
        List<TicketData> signature = new()
        {
            new BinaryData("bruh"u8.ToArray()),
            new BinaryData(new byte[0x38]),
        };
        TicketData footerBlob = new BlobData(2, signature);
        MemoryStream ms = new();
        BinaryWriter writer = new(ms);
        writer.Write(new byte[]{0x21, 0x01, 0x00, 0x00,});
        int ticketLen = userBlob.Len() + footerBlob.Len() + 8;
        byte[] lenAsBytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(lenAsBytes, (uint)ticketLen);
        writer.Write(lenAsBytes);
        userBlob.Write(writer);
        footerBlob.Write(writer);
        return ms.ToArray();
    }

    public TicketBuilder SetUsername(string username)
    {
        this.Username = username;
        return this;
    }

    public TicketBuilder SetTitleId(string titleId)
    {
        this.TitleId = titleId;
        return this;
    }

    public TicketBuilder setExpirationTime(ulong expirationTime)
    {
        this.ExpirationTime = expirationTime;
        return this;
    }

    public TicketBuilder SetIssueTime(ulong issueTime)
    {
        this.IssueTime = issueTime;
        return this;
    }

    public TicketBuilder SetIssuerId(ushort issuerId)
    {
        this.IssuerId = issuerId;
        return this;
    }

    public TicketBuilder SetUserId(ulong userId)
    {
        this.UserId = userId;
        return this;
    }

    public TicketBuilder SetCountry(string country)
    {
        this.Country = country;
        return this;
    }

    public TicketBuilder SetDomain(string domain)
    {
        this.Domain = domain;
        return this;
    }

    public TicketBuilder SetStatus(uint status)
    {
        this.Status = status;
        return this;
    }

}