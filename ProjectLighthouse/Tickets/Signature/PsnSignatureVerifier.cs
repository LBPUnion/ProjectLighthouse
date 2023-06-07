using System;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace LBPUnion.ProjectLighthouse.Tickets.Signature;

public class PsnSignatureVerifier : TicketSignatureVerifier
{
    private static readonly Lazy<ECPublicKeyParameters> publicKeyParams = new(() =>
    {
        ECDomainParameters curve = FromX9EcParams(ECNamedCurveTable.GetByName("secp192r1"));
        ECPoint publicKey = curve.Curve.CreatePoint(
            new BigInteger("39c62d061d4ee35c5f3f7531de0af3cf918346526edac727", 16),
            new BigInteger("a5d578b55113e612bf1878d4cc939d61a41318403b5bdf86", 16));
        return new ECPublicKeyParameters(publicKey, curve);
    });

    protected override ECPublicKeyParameters PublicKey => publicKeyParams.Value;
    protected override string HashAlgorithm => "SHA-1";

    private readonly NPTicket ticket;

    public PsnSignatureVerifier(NPTicket ticket)
    {
        this.ticket = ticket;
    }

    protected override bool VerifySignature(ISigner signer)
    {
        Span<byte> ticketBody = this.ticket.Data.AsSpan();
        ticketBody = ticketBody[..ticketBody.IndexOf(this.ticket.TicketSignature)];
        signer.BlockUpdate(ticketBody);

        return signer.VerifySignature(TrimSignature(this.ticket.TicketSignature));
    }    
}