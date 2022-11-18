#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Tickets.Data;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Version = LBPUnion.ProjectLighthouse.Types.Version;

namespace LBPUnion.ProjectLighthouse.Tickets;

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

    private byte[] ticketBody { get; set; } = Array.Empty<byte>();

    private byte[] ticketSignature { get; set; } = Array.Empty<byte>();

    public GameVersion GameVersion { get; set; }

    private static readonly ECDomainParameters secp224K1 = FromX9EcParams(ECNamedCurveTable.GetByName("secp224k1"));
    private static readonly ECDomainParameters secp192K1 = FromX9EcParams(ECNamedCurveTable.GetByName("secp192k1"));

    private static readonly ECPoint rpcnPublic = secp224K1.Curve.CreatePoint(
        new BigInteger("b07bc0f0addb97657e9f389039e8d2b9c97dc2a31d3042e7d0479b93", 16),
        new BigInteger("d81c42b0abdf6c42191a31e31f93342f8f033bd529c2c57fdb5a0a7d", 16));

    private ECDomainParameters getCurveParams() => this.IsRpcn() ? secp224K1 : secp192K1;

    private static ECPoint getPublicKey() => rpcnPublic;

    private static ECDomainParameters FromX9EcParams(X9ECParameters param) =>
        new(param.Curve, param.G, param.N, param.H, param.GetSeed());

    private bool ValidateSignature()
    {
        //TODO support psn
        if (!this.IsRpcn()) return true;

        ECPublicKeyParameters pubKey = new(getPublicKey(), this.getCurveParams());
        ISigner signer = SignerUtilities.GetSigner("SHA-224withECDSA");
        signer.Init(false, pubKey);

        signer.BlockUpdate(this.ticketBody);

        return signer.VerifySignature(this.ticketSignature);
    }

    private bool IsRpcn() => this.IssuerId == 0x33333333; 

    private static readonly Dictionary<Platform, byte[]> identifierByPlatform = new()
    {
        {
            Platform.RPCS3, new byte[] { 0x52, 0x50, 0x43, 0x4E, }
        },
        {
            Platform.PS3, new byte[]{ 0x71, 0x9F, 0x1D, 0x4A, }
        }
    };

    private static bool Read21Ticket(NPTicket npTicket, TicketReader reader)
    {
        reader.ReadTicketString(); // "Serial id", but its apparently not what we're looking for

        npTicket.IssuerId = reader.ReadTicketUInt32();
        npTicket.IssuedDate = reader.ReadTicketUInt64();
        npTicket.ExpireDate = reader.ReadTicketUInt64();

        ulong uid = reader.ReadTicketUInt64(); // PSN User id, we don't care about this
        Console.WriteLine(@$"npTicket uid = {uid}");

        npTicket.Username = reader.ReadTicketString();

        reader.ReadTicketString(); // Country
        reader.ReadTicketString(); // Domain

        npTicket.titleId = reader.ReadTicketString();

        reader.ReadTicketUInt32(); // status

        reader.ReadTicketEmpty(); // padding
        reader.ReadTicketEmpty();

        reader.ReadSectionHeader(); // footer header

        byte[] ident = reader.ReadTicketBinary(); // 4 byte identifier
        Platform platform = npTicket.IsRpcn() ? Platform.RPCS3 : Platform.PS3;
        if (!ident.SequenceEqual(identifierByPlatform[platform]))
        {
            Console.WriteLine(@$"Identity sequence mismatch, platform={npTicket.Platform} - {Convert.ToHexString(ident)} == {Convert.ToHexString(identifierByPlatform[npTicket.Platform])}");
            return false;
        }
        
        //TODO check platform and ident

        npTicket.ticketSignature = reader.ReadTicketBinary();
        return true;
    }

    // Function is here for future use incase we ever need to read more from the ticket
    private static bool Read30Ticket(NPTicket npTicket, TicketReader reader) => Read21Ticket(npTicket, reader);

    private static bool ReadTicket(byte[] data, NPTicket npTicket, TicketReader reader)
    {
        npTicket.ticketVersion = reader.ReadTicketVersion();

        reader.ReadBytes(4); // Skip header

        ushort ticketLen = reader.ReadUInt16BE(); // Ticket length, we don't care about this
        if (ticketLen != data.Length - 0x8)
        {
            Console.WriteLine("Ticket length mismatch");
            return false;
        }

        long bodyStart = reader.BaseStream.Position;
        SectionHeader bodyHeader = reader.ReadSectionHeader();
        
        npTicket.ticketBody = data.AsSpan().Slice((int)bodyStart, bodyHeader.Length+4).ToArray();
        
        Logger.Debug($"bodyHeader.Type is {bodyHeader.Type}, index={bodyStart}", LogArea.Login);

        return npTicket.ticketVersion.ToString() switch
        {
            "2.1" => Read21Ticket(npTicket, reader),
            "3.0" => Read30Ticket(npTicket, reader),
            _ => throw new NotImplementedException(),
        };
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
                    Username = dataStr["unitTestTicket".Length..],
                };

                return npTicket;
            }
        }
        #endif
        try
        {
            using MemoryStream ms = new(data);
            using TicketReader reader = new(ms);

            bool validTicket = ReadTicket(data, npTicket, reader);
            if (!validTicket)
            {
                Logger.Warn($"Failed to parse ticket from {npTicket.Username}", LogArea.Login);
                return null;
            }

            if (npTicket.titleId == null) throw new ArgumentNullException($"{nameof(npTicket)}.{nameof(npTicket.titleId)}");

            // We already read the title id, however we need to do some post-processing to get what we want.
            // Current data: UP9000-BCUS98245_00
            // We need to chop this to get the titleId we're looking for
            npTicket.titleId = npTicket.titleId[7..]; // Trim UP9000-
            npTicket.titleId = npTicket.titleId[..^3]; // Trim _00 at the end
            // Data now (hopefully): BCUS98245

            Logger.Debug($"titleId is {npTicket.titleId}", LogArea.Login);

            npTicket.GameVersion = GameVersionHelper.FromTitleId(npTicket.titleId); // Finally, convert it to GameVersion

            if (npTicket.GameVersion == GameVersion.Unknown)
            {
                Logger.Warn($"Could not determine game version from title id {npTicket.titleId}", LogArea.Login);
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
                Logger.Warn($"Could not determine platform from IssuerId {npTicket.IssuerId} decimal", LogArea.Login);
                return null;
            }

            bool valid = npTicket.ValidateSignature();
            if (!valid)
            {
                Logger.Warn($"Failed to verify authenticity of ticket from user {npTicket.Username}", LogArea.Login);
                return null;
            }

            Logger.Success($"Verified ticket signature from {npTicket.Username}", LogArea.Login);

            #if DEBUG
            Logger.Debug("npTicket data:", LogArea.Login);
            Logger.Debug(JsonSerializer.Serialize(npTicket), LogArea.Login);
            #endif

            return npTicket;
        }
        catch(NotImplementedException)
        {
            Logger.Error($"The ticket version {npTicket.ticketVersion} is not implemented yet.", LogArea.Login);
            Logger.Error
            (
                "Please let us know that this is a ticket version that is actually used on our issue tracker at https://github.com/LBPUnion/project-lighthouse/issues !",
                LogArea.Login
            );

            return null;
        }
        catch(Exception e)
        {
            Logger.Error("Failed to read npTicket!", LogArea.Login);
            Logger.Error("Either this is spam data, or the more likely that this is a bug.", LogArea.Login);
            Logger.Error
                ("Please report the following exception to our issue tracker at https://github.com/LBPUnion/project-lighthouse/issues!", LogArea.Login);
            Logger.Error(e.ToDetailedException(), LogArea.Login);
            return null;
        }
    }
}