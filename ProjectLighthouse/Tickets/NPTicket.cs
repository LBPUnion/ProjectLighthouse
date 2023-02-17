#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Users;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
#if DEBUG
using System.Text.Json;
#endif

namespace LBPUnion.ProjectLighthouse.Tickets;

/// <summary>
///     A PSN ticket, typically sent from PS3/RPCN
/// </summary>
public class NPTicket
{
    public string? Username { get; set; }

    private TicketVersion? ticketVersion { get; set; }

    public Platform Platform { get; set; }

    public uint IssuerId { get; set; }
    public ulong IssuedDate { get; set; }
    public ulong ExpireDate { get; set; }
    public ulong UserId { get; set; }
    public string TicketHash { get; set; } = "";

    private string? titleId { get; set; }

    private byte[] ticketBody { get; set; } = Array.Empty<byte>();

    private byte[] ticketSignature { get; set; } = Array.Empty<byte>();
    private byte[] ticketSignatureIdentifier { get; set; } = Array.Empty<byte>();

    public GameVersion GameVersion { get; set; }

    private static ECDomainParameters FromX9EcParams(X9ECParameters param) =>
        new(param.Curve, param.G, param.N, param.H, param.GetSeed());

    public static readonly ECDomainParameters Secp224K1 = FromX9EcParams(ECNamedCurveTable.GetByName("secp224k1"));
    public static readonly ECDomainParameters Secp192R1 = FromX9EcParams(ECNamedCurveTable.GetByName("secp192r1"));

    private static readonly ECPoint rpcnPublic = Secp224K1.Curve.CreatePoint(
        new BigInteger("b07bc0f0addb97657e9f389039e8d2b9c97dc2a31d3042e7d0479b93", 16),
        new BigInteger("d81c42b0abdf6c42191a31e31f93342f8f033bd529c2c57fdb5a0a7d", 16));

    private static readonly ECPoint psnPublic = Secp192R1.Curve.CreatePoint(
        new BigInteger("39c62d061d4ee35c5f3f7531de0af3cf918346526edac727", 16),
        new BigInteger("a5d578b55113e612bf1878d4cc939d61a41318403b5bdf86", 16));

    private static readonly ECPoint unitTestPublic = Secp192R1.Curve.CreatePoint(
        new BigInteger("b6f3374bde4ec23a25e1508889e7d7e71870ba74daf8654f", 16),
        new BigInteger("738de93dad0fffb5642045439afaaf8c6fda319a72d2a584", 16));

    internal class SignatureParams
    {
        public string HashAlgo { get; set; }
        public ECPoint PublicKey { get; set; }
        public ECDomainParameters CurveParams { get; set; }

        public SignatureParams(string hashAlgo, ECPoint pubKey, ECDomainParameters curve)
        {
            this.HashAlgo = hashAlgo;
            this.PublicKey = pubKey;
            this.CurveParams = curve;
        }
    }

    private readonly Dictionary<string, SignatureParams> signatureParamsMap = new()
    {
        //psn
        { "719F1D4A", new SignatureParams("SHA-1", psnPublic, Secp192R1) },
        //rpcn
        { "5250434E", new SignatureParams("SHA-224", rpcnPublic, Secp224K1) },
        //unit test
        { "54455354", new SignatureParams("SHA-1", unitTestPublic, Secp192R1) },
    };

    private bool ValidateSignature()
    {
        string identifierHex = Convert.ToHexString(this.ticketSignatureIdentifier);
        if (!this.signatureParamsMap.ContainsKey(identifierHex))
        {
            Logger.Warn($"Unknown signature identifier in ticket: {identifierHex}, platform={this.Platform}", LogArea.Login);
            return false;
        }

        SignatureParams sigParams = this.signatureParamsMap[identifierHex];
        ECPublicKeyParameters pubKey = new(sigParams.PublicKey, sigParams.CurveParams);

        ISigner signer = SignerUtilities.GetSigner($"{sigParams.HashAlgo}withECDSA");
        signer.Init(false, pubKey);

        signer.BlockUpdate(this.ticketBody);

        return signer.VerifySignature(this.ticketSignature);
    }

    // Sometimes psn signatures have one or two extra empty bytes
    // This is slow but it's better than carelessly chopping 0's
    private static byte[] ParseSignature(byte[] signature)
    {
        for (int i = 0; i <= 2; i++)
        {
            try
            {
                Asn1Object.FromByteArray(signature);
                break;
            }
            catch
            {
                signature = signature.SkipLast(1).ToArray();
            }
        }

        return signature;
    }

    private static bool Read21Ticket(NPTicket npTicket, TicketReader reader)
    {
        reader.ReadTicketString(); // serial id

        npTicket.IssuerId = reader.ReadTicketUInt32();
        npTicket.IssuedDate = reader.ReadTicketUInt64();
        npTicket.ExpireDate = reader.ReadTicketUInt64();

        npTicket.UserId = reader.ReadTicketUInt64();

        npTicket.Username = reader.ReadTicketString();

        reader.ReadTicketString(); // Country
        reader.ReadTicketString(); // Domain

        npTicket.titleId = reader.ReadTicketString();

        reader.ReadTicketUInt32(); // status

        reader.ReadTicketEmpty(); // padding
        reader.ReadTicketEmpty();

        SectionHeader footer = reader.ReadSectionHeader(); // footer header
        if (footer.Type != SectionType.Footer)
        {
            Logger.Warn(@$"Unexpected ticket footer header: expected={SectionType.Footer}, actual={footer}",
                LogArea.Login);
            return false;
        }

        npTicket.ticketSignatureIdentifier = reader.ReadTicketBinary();

        npTicket.ticketSignature = ParseSignature(reader.ReadTicketBinary());
        return true;
    }

    private static bool Read30Ticket(NPTicket npTicket, TicketReader reader)
    {
        reader.ReadTicketString(); // serial id

        npTicket.IssuerId = reader.ReadTicketUInt32();
        npTicket.IssuedDate = reader.ReadTicketUInt64();
        npTicket.ExpireDate = reader.ReadTicketUInt64();

        npTicket.UserId = reader.ReadTicketUInt64();

        npTicket.Username = reader.ReadTicketString();

        reader.ReadTicketString(); // Country
        reader.ReadTicketString(); // Domain

        npTicket.titleId = reader.ReadTicketString();

        reader.ReadSectionHeader(); // date of birth section
        reader.ReadBytes(4); // 4 bytes for year month and day
        reader.ReadTicketUInt32();

        reader.ReadSectionHeader(); // empty section?
        reader.ReadTicketEmpty();

        SectionHeader footer = reader.ReadSectionHeader(); // footer header
        if (footer.Type != SectionType.Footer)
        {
            Logger.Warn(@$"Unexpected ticket footer header: expected={SectionType.Footer}, actual={footer}",
                LogArea.Login);
            return false;
        }

        npTicket.ticketSignatureIdentifier = reader.ReadTicketBinary();

        npTicket.ticketSignature = ParseSignature(reader.ReadTicketBinary());
        return true;
    }

    private static bool ReadTicket(byte[] data, NPTicket npTicket, TicketReader reader)
    {
        npTicket.ticketVersion = reader.ReadTicketVersion();

        reader.ReadBytes(4); // Skip header

        ushort ticketLen = reader.ReadUInt16BE();
        // Subtract 8 bytes to account for ticket header
        if (ticketLen != data.Length - 0x8)
        {
            Logger.Warn(@$"Ticket length mismatch, expected={ticketLen}, actual={data.Length - 0x8}", LogArea.Login);
            return false;
        }

        long bodyStart = reader.BaseStream.Position;
        SectionHeader bodyHeader = reader.ReadSectionHeader();

        if (bodyHeader.Type != SectionType.Body)
        {
            Logger.Warn(@$"Unexpected ticket body header: expected={SectionType.Body}, actual={bodyHeader}", LogArea.Login);
            return false;
        }

        Logger.Debug($"bodyHeader.Type is {bodyHeader.Type}, index={bodyStart}", LogArea.Login);

        bool parsedSuccessfully = npTicket.ticketVersion.ToString() switch
        {
            "2.1" => Read21Ticket(npTicket, reader), // used by ps3 and rpcs3
            "3.0" => Read30Ticket(npTicket, reader), // used by ps vita
            _ => throw new NotImplementedException(),
        };

        if (!parsedSuccessfully) return false;

        npTicket.ticketBody = Convert.ToHexString(npTicket.ticketSignatureIdentifier) switch
        {
            // rpcn
            "5250434E" => data.AsSpan().Slice((int)bodyStart, bodyHeader.Length + 4).ToArray(),
            // psn and unit test
            "719F1D4A" or "54455354" => data.AsSpan()[..data.AsSpan().IndexOf(npTicket.ticketSignature)].ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(npTicket)),
        };

        return true;
    }

    /// <summary>
    ///     https://www.psdevwiki.com/ps3/X-I-5-Ticket
    /// </summary>
    public static NPTicket? CreateFromBytes(byte[] data)
    {
        NPTicket npTicket = new();
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

            if (npTicket.IssuedDate > (ulong)TimeHelper.TimestampMillis)
            {
                Logger.Warn($"Ticket isn't valid yet from {npTicket.Username} ({npTicket.IssuedDate} > {(ulong)TimeHelper.TimestampMillis})", LogArea.Login);
                return null;
            }
            if ((ulong)TimeHelper.TimestampMillis > npTicket.ExpireDate)
            {
                Logger.Warn($"Ticket has expired from {npTicket.Username} ({(ulong)TimeHelper.TimestampMillis} > {npTicket.ExpireDate}", LogArea.Login);
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
                0x74657374 => Platform.UnitTest,
                _ => Platform.Unknown,
            };

            if (npTicket.Platform == Platform.PS3 && npTicket.GameVersion == GameVersion.LittleBigPlanetVita) npTicket.Platform = Platform.Vita;

            if (npTicket.Platform == Platform.Unknown || (npTicket.Platform == Platform.UnitTest && !ServerStatics.IsUnitTesting))
            {
                Logger.Warn($"Could not determine platform from IssuerId {npTicket.IssuerId} decimal", LogArea.Login);
                return null;
            }

            if (ServerConfiguration.Instance.Authentication.VerifyTickets && !npTicket.ValidateSignature())
            {
                Logger.Warn($"Failed to verify authenticity of ticket from user {npTicket.Username}", LogArea.Login);
                return null;
            }

            // Used to identify duplicate tickets
            npTicket.TicketHash = CryptoHelper.Sha1Hash(data);

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
