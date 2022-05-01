#nullable enable
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Types.Tickets;

/// <summary>
///     A PSN ticket, typically sent from PS3/RPCN
/// </summary>
public class NPTicket
{
    public string? Username { get; set; }

    private Version? ticketVersion { get; set; }

    public Platform Platform { get; set; }

    public uint IssuerId { get; set; }
    public ulong IssuedDate { get; set; }
    public ulong ExpireDate { get; set; }

    private string? titleId { get; set; }

    public GameVersion GameVersion { get; set; }

    private static void Read21Ticket(NPTicket npTicket, TicketReader reader)
    {
        reader.ReadTicketString(); // "Serial id", but its apparently not what we're looking for

        npTicket.IssuerId = reader.ReadTicketUInt32();
        npTicket.IssuedDate = reader.ReadTicketUInt64();
        npTicket.ExpireDate = reader.ReadTicketUInt64();

        reader.ReadTicketUInt64(); // PSN User id, we don't care about this

        npTicket.Username = reader.ReadTicketString();

        reader.ReadTicketString(); // Country
        reader.ReadTicketString(); // Domain

        npTicket.titleId = reader.ReadTicketString();
    }

    // Function is here for future use incase we ever need to read more from the ticket
    private static void Read30Ticket(NPTicket npTicket, TicketReader reader)
    {
        Read21Ticket(npTicket, reader);
    }

    /// <summary>
    ///     https://www.psdevwiki.com/ps3/X-I-5-Ticket
    /// </summary>
    public static NPTicket? CreateFromBytes(byte[] data)
    {
        NPTicket npTicket = new();
        #if DEBUG
        if (data[0] == 'u' && ServerStatics.IsUnitTesting)
        {
            string dataStr = Encoding.UTF8.GetString(data);
            if (dataStr.StartsWith("unitTestTicket"))
            {
                npTicket = new NPTicket
                {
                    IssuerId = 0,
                    ticketVersion = new Version(0, 0),
                    Platform = Platform.UnitTest,
                    GameVersion = GameVersion.LittleBigPlanet2,
                    ExpireDate = 0,
                    IssuedDate = 0,
                };

                npTicket.Username = dataStr.Substring(14);

                return npTicket;
            }
        }
        #endif
        try
        {
            using MemoryStream ms = new(data);
            using TicketReader reader = new(ms);

            npTicket.ticketVersion = reader.ReadTicketVersion();

            reader.ReadBytes(4); // Skip header

            reader.ReadUInt16BE(); // Ticket length, we don't care about this

            SectionHeader bodyHeader = reader.ReadSectionHeader();
            Logger.LogDebug($"bodyHeader.Type is {bodyHeader.Type}", LogArea.Login);

            switch (npTicket.ticketVersion)
            {
                case "2.1":
                    Read21Ticket(npTicket, reader);
                    break;
                case "3.0":
                    Read30Ticket(npTicket, reader);
                    break;
                default: throw new NotImplementedException();
            }

            if (npTicket.titleId == null) throw new ArgumentNullException($"{nameof(npTicket)}.{nameof(npTicket.titleId)}");

            // We already read the title id, however we need to do some post-processing to get what we want.
            // Current data: UP9000-BCUS98245_00
            // We need to chop this to get the titleId we're looking for
            npTicket.titleId = npTicket.titleId.Substring(7); // Trim UP9000-
            npTicket.titleId = npTicket.titleId.Substring(0, npTicket.titleId.Length - 3); // Trim _00 at the end
            // Data now (hopefully): BCUS98245

            Logger.LogDebug($"titleId is {npTicket.titleId}", LogArea.Login);

            npTicket.GameVersion = GameVersionHelper.FromTitleId(npTicket.titleId); // Finally, convert it to GameVersion

            if (npTicket.GameVersion == GameVersion.Unknown)
            {
                Logger.LogWarn($"Could not determine game version from title id {npTicket.titleId}", LogArea.Login);
                return null;
            }

            // Production PSN Issuer ID: 0x100
            // RPCN Issuer ID:           0x33333333
            npTicket.Platform = npTicket.IssuerId switch
            {
                0x100 => Platform.PS3,
                0x33333333 => Platform.RPCS3,
                _ => Platform.Unknown,
            };

            if (npTicket.Platform == Platform.PS3 && npTicket.GameVersion == GameVersion.LittleBigPlanetVita) npTicket.Platform = Platform.Vita;

            if (npTicket.Platform == Platform.Unknown)
            {
                Logger.LogWarn($"Could not determine platform from IssuerId {npTicket.IssuerId} decimal", LogArea.Login);
                return null;
            }

            #if DEBUG
            Logger.LogDebug("npTicket data:", LogArea.Login);
            Logger.LogDebug(JsonSerializer.Serialize(npTicket), LogArea.Login);
            #endif

            return npTicket;
        }
        catch(NotImplementedException)
        {
            Logger.LogError($"The ticket version {npTicket.ticketVersion} is not implemented yet.", LogArea.Login);
            Logger.LogError
            (
                "Please let us know that this is a ticket version that is actually used on our issue tracker at https://github.com/LBPUnion/project-lighthouse/issues !",
                LogArea.Login
            );

            return null;
        }
        catch(Exception e)
        {
            Logger.LogError("Failed to read npTicket!", LogArea.Login);
            Logger.LogError("Either this is spam data, or the more likely that this is a bug.", LogArea.Login);
            Logger.LogError
                ("Please report the following exception to our issue tracker at https://github.com/LBPUnion/project-lighthouse/issues!", LogArea.Login);
            Logger.LogError(e.ToDetailedException(), LogArea.Login);
            return null;
        }
    }
}