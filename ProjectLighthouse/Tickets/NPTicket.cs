#nullable enable
using System;
using System.IO;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Tickets.Parser;
using LBPUnion.ProjectLighthouse.Tickets.Signature;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Users;
#if DEBUG
using System.Text.Json;
#endif

namespace LBPUnion.ProjectLighthouse.Tickets;

/// <summary>
///     A PSN ticket, typically sent from PS3/RPCN
/// </summary>
public class NPTicket
{
    public string Username { get; protected internal set; } = "";

    public Platform Platform { get; private set; }
    public GameVersion GameVersion { get; private set; }
    public string TicketHash { get; private set; } = "";

    public uint IssuerId { get; set; }
    public ulong IssueTime { get; set; }
    public ulong ExpireTime { get; set; }
    public ulong UserId { get; set; }

    private TicketVersion? TicketVersion { get; set; }

    protected internal string TitleId { get; set; } = "";

    private TicketSignatureVerifier SignatureVerifier { get; set; } = new NullSignatureVerifier();

    protected internal SectionHeader BodyHeader { get; set; }

    protected internal byte[] Data { get; set; } = Array.Empty<byte>();
    protected internal byte[] TicketSignature { get; set; } = Array.Empty<byte>();
    protected internal byte[] TicketSignatureIdentifier { get; set; } = Array.Empty<byte>();

    private static bool ReadTicket(NPTicket npTicket, TicketReader reader)
    {
        npTicket.TicketVersion = reader.ReadTicketVersion();

        reader.SkipBytes(4); // Skip header

        ushort ticketLen = reader.ReadUInt16BE();

        // Subtract 8 bytes to account for ticket header
        if (ticketLen != npTicket.Data.Length - 0x8)
        {
            throw new TicketParseException(
                @$"Ticket length mismatch, expected={ticketLen}, actual={npTicket.Data.Length - 0x8}");
        }

        npTicket.BodyHeader = reader.ReadSectionHeader();

        if (npTicket.BodyHeader.Type != SectionType.Body)
        {
            throw new TicketParseException(
                @$"Unexpected ticket body header: expected={SectionType.Body}, actual={npTicket.BodyHeader.Type}");
        }

        Logger.Debug($"bodyHeader.Type is {npTicket.BodyHeader.Type}, index={npTicket.BodyHeader.Position}",
            LogArea.Login);

        ITicketParser ticketParser = npTicket.TicketVersion switch
        {
            Tickets.TicketVersion.V21 => new TicketParser21(npTicket, reader), // used by ps3 and rpcs3
            Tickets.TicketVersion.V30 => new TicketParser30(npTicket, reader), // used by ps vita
            _ => throw new NotImplementedException(),
        };

        if (!ticketParser.ParseTicket()) return false;

        npTicket.SignatureVerifier = npTicket.TicketSignatureIdentifier switch
        {
            [0x71, 0x9F, 0x1D, 0x4A,] => new PsnSignatureVerifier(npTicket), // PSN LBP Signature Identifier
            [0x52, 0x50, 0x43, 0x4E,] => new RpcnSignatureVerifier(npTicket), // 'RPCN' in ascii
            [0x54, 0x45, 0x53, 0x54,] => new UnitTestSignatureVerifier(npTicket), // 'TEST' in ascii
            _ => throw new ArgumentOutOfRangeException(nameof(npTicket),
                npTicket.TicketSignatureIdentifier,
                @"Invalid signature identifier"),
        };

        return true;
    }

    /// <summary>
    ///     https://www.psdevwiki.com/ps3/X-I-5-Ticket
    /// </summary>
    public static NPTicket? CreateFromBytes(byte[] data)
    {
        // Header should have at least 8 bytes
        if (data.Length < 8)
        {
            Logger.Warn("NpTicket does not contain header", LogArea.Login);
            return null;
        }
        NPTicket npTicket = new()
        {
            Data = data,
        };
        try
        {
            using TicketReader reader = new(new MemoryStream(data));

            bool validTicket = ReadTicket(npTicket, reader);
            if (!validTicket)
            {
                Logger.Warn($"Failed to parse ticket from {npTicket.Username}", LogArea.Login);
                return null;
            }

            if (npTicket.IssueTime > (ulong)TimeHelper.TimestampMillis)
            {
                Logger.Warn(
                    $"Ticket isn't valid yet from {npTicket.Username} ({npTicket.IssueTime} > {(ulong)TimeHelper.TimestampMillis})",
                    LogArea.Login);
                return null;
            }

            if ((ulong)TimeHelper.TimestampMillis > npTicket.ExpireTime)
            {
                Logger.Warn(
                    $"Ticket has expired from {npTicket.Username} ({(ulong)TimeHelper.TimestampMillis} > {npTicket.ExpireTime}",
                    LogArea.Login);
                return null;
            }

            if (npTicket.TitleId == null)
                throw new ArgumentNullException($"{nameof(npTicket)}.{nameof(npTicket.TitleId)}");

            // We already read the title id, however we need to do some post-processing to get what we want.
            // Current data: UP9000-BCUS98245_00
            // We need to chop this to get the titleId we're looking for
            npTicket.TitleId = npTicket.TitleId[7..]; // Trim UP9000-
            npTicket.TitleId = npTicket.TitleId[..^3]; // Trim _00 at the end
            // Data now (hopefully): BCUS98245

            Logger.Debug($"titleId is {npTicket.TitleId}", LogArea.Login);

            npTicket.GameVersion = GameVersionHelper.FromTitleId(npTicket.TitleId); // Finally, convert it to GameVersion

            if (npTicket.GameVersion == GameVersion.Unknown)
            {
                Logger.Warn($"Could not determine game version from title id {npTicket.TitleId}", LogArea.Login);
                return null;
            }

            npTicket.Platform = npTicket.SignatureVerifier switch
            {
                PsnSignatureVerifier => Platform.PS3,
                RpcnSignatureVerifier => Platform.RPCS3,
                UnitTestSignatureVerifier => Platform.UnitTest,
                _ => Platform.Unknown,
            };

            if (npTicket.GameVersion == GameVersion.LittleBigPlanetVita) npTicket.Platform = Platform.Vita;

            if (npTicket.Platform == Platform.Unknown)
            {
                Logger.Warn($"Could not determine platform from IssuerId {npTicket.IssuerId} decimal", LogArea.Login);
                return null;
            }

            if (ServerConfiguration.Instance.Authentication.VerifyTickets &&
                !npTicket.SignatureVerifier.ValidateSignature())
            {
                Logger.Warn($"Failed to verify authenticity of ticket from user {npTicket.Username}", LogArea.Login);
                return null;
            }

            // Used to identify duplicate tickets
            npTicket.TicketHash = CryptoHelper.Sha256Hash(data);

            #if DEBUG
            Logger.Debug("npTicket data:", LogArea.Login);
            Logger.Debug(JsonSerializer.Serialize(npTicket), LogArea.Login);
            #endif

            return npTicket;
        }
        catch (TicketParseException e)
        {
            Logger.Error($"Parsing npTicket failed: {e.Message}", LogArea.Login);
            return null;
        }
        catch(NotImplementedException)
        {
            Logger.Error($"The ticket version {npTicket.TicketVersion} is not implemented yet.", LogArea.Login);
            Logger.Error
            (
                "Please let us know that this is a ticket version that is actually used on our issue tracker at https://github.com/LBPUnion/ProjectLighthouse/issues !",
                LogArea.Login
            );
            return null;
        }
        catch(Exception e)
        {
            Logger.Error("Failed to read npTicket!", LogArea.Login);
            Logger.Error("Either this is spam data, or the more likely that this is a bug.", LogArea.Login);
            Logger.Error
                ("Please report the following exception to our issue tracker at https://github.com/LBPUnion/ProjectLighthouse/issues!", LogArea.Login);
            Logger.Error(e.ToDetailedException(), LogArea.Login);
            return null;
        }
    }
}
