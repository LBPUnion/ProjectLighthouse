using System;
using LBPUnion.ProjectLighthouse.Configuration;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace LBPUnion.ProjectLighthouse.Tickets.Signature;

public class UnitTestSignatureVerifier : TicketSignatureVerifier
{
    protected internal static readonly Lazy<ECPublicKeyParameters> PublicKeyParams = new(() =>
    {
        ECDomainParameters curve = FromX9EcParams(ECNamedCurveTable.GetByName("secp192r1"));
        ECPoint publicKey = curve.Curve.CreatePoint(
            new BigInteger("b6f3374bde4ec23a25e1508889e7d7e71870ba74daf8654f", 16),
            new BigInteger("738de93dad0fffb5642045439afaaf8c6fda319a72d2a584", 16));
        return new ECPublicKeyParameters(publicKey, curve);
    });

    protected override ECPublicKeyParameters PublicKey => PublicKeyParams.Value;
    protected override string HashAlgorithm => "SHA-1";

    private readonly NPTicket ticket;

    public UnitTestSignatureVerifier(NPTicket ticket)
    {
        this.ticket = ticket;
    }

    protected override bool VerifySignature(ISigner signer)
    {
        // Don't allow tickets signed by the unit testing private key
        if (!ServerStatics.IsUnitTesting) return false;

        Span<byte> ticketBody = this.ticket.Data.AsSpan();
        ticketBody = ticketBody[..ticketBody.IndexOf(this.ticket.TicketSignature)];
        signer.BlockUpdate(ticketBody);

        return signer.VerifySignature(TrimSignature(this.ticket.TicketSignature));
    }
}