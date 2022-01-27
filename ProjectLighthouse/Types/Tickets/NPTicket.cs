#nullable enable
using System;
using System.IO;
using System.Text;
using Kettu;
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
    public string Username { get; set; }

    private Version ticketVersion { get; set; }

    public Platform Platform { get; set; }

    public uint IssuerId { get; set; }
    public ulong IssuedDate { get; set; }
    public ulong ExpireDate { get; set; }

    public GameVersion GameVersion { get; set; }

    /// <summary>
    ///     https://www.psdevwiki.com/ps3/X-I-5-Ticket
    /// </summary>
    public static NPTicket? CreateFromBytes(byte[] data)
    {
        #if DEBUG
        if (data[0] == 'u' && ServerStatics.IsUnitTesting)
        {
            string dataStr = Encoding.UTF8.GetString(data);
            if (dataStr.StartsWith("unitTestTicket"))
            {
                NPTicket npTicket = new()
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
            NPTicket npTicket = new();
            using MemoryStream ms = new(data);
            using TicketReader reader = new(ms);

            npTicket.ticketVersion = reader.ReadTicketVersion();

            reader.ReadBytes(4); // Skip header

            reader.ReadUInt16BE(); // Ticket length, we don't care about this

            if (npTicket.ticketVersion != "2.1") throw new NotImplementedException();

            #if DEBUG
            SectionHeader bodyHeader = reader.ReadSectionHeader();
            Logger.Log($"bodyHeader.Type is {bodyHeader.Type}", LoggerLevelLogin.Instance);
            #else
            reader.ReadSectionHeader();
            #endif

            reader.ReadTicketString(); // "Serial id", but its apparently not what we're looking for

            npTicket.IssuerId = reader.ReadTicketUInt32();
            npTicket.IssuedDate = reader.ReadTicketUInt64();
            npTicket.ExpireDate = reader.ReadTicketUInt64();

            reader.ReadTicketUInt64(); // PSN User id, we don't care about this

            npTicket.Username = reader.ReadTicketString();

            reader.ReadTicketString(); // Country
            reader.ReadTicketString(); // Domain

            // Title ID, kinda..
            // Data: "UP9000-BCUS98245_00
            string titleId = reader.ReadTicketString();
            titleId = titleId.Substring(7); // Trim UP9000-
            titleId = titleId.Substring(0, titleId.Length - 3); // Trim _00 at the end

            #if DEBUG
            Logger.Log($"titleId is {titleId}", LoggerLevelLogin.Instance);
            #endif

            npTicket.GameVersion = GameVersionHelper.FromTitleId(titleId); // Finally, convert it to GameVersion

            // Production PSN Issuer ID: 0x100
            // RPCN Issuer ID:           0x33333333
            npTicket.Platform = npTicket.IssuerId switch
            {
                0x100 => Platform.PS3,
                0x33333333 => Platform.RPCS3,
                _ => Platform.Unknown,
            };

            if (npTicket.Platform == Platform.Unknown)
            {
                Logger.Log($"Could not determine platform from IssuerId {npTicket.IssuerId} decimal", LoggerLevelLogin.Instance);
                return null;
            }

            if (npTicket.GameVersion == GameVersion.Unknown)
            {
                Logger.Log($"Could not determine game version from title id {titleId}", LoggerLevelLogin.Instance);
                return null;
            }

            return npTicket;
        }
        catch
        {
            return null;
        }
    }
}