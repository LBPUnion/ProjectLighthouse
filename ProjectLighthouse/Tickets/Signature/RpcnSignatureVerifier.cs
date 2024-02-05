using System;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace LBPUnion.ProjectLighthouse.Tickets.Signature;

public class RpcnSignatureVerifier : TicketSignatureVerifier
{
    private static readonly Lazy<ECPublicKeyParameters> publicKeyParams = new(() =>
    {
        ECDomainParameters curve = FromX9EcParams(ECNamedCurveTable.GetByName("secp224k1"));
        ECPoint publicKey = curve.Curve.CreatePoint(
            new BigInteger("b07bc0f0addb97657e9f389039e8d2b9c97dc2a31d3042e7d0479b93", 16),
            new BigInteger("d81c42b0abdf6c42191a31e31f93342f8f033bd529c2c57fdb5a0a7d", 16));
        return new ECPublicKeyParameters(publicKey, curve);
    });

    protected override ECPublicKeyParameters PublicKey => publicKeyParams.Value;
    protected override string HashAlgorithm => "SHA-224";

    private readonly NPTicket ticket;

    public RpcnSignatureVerifier(NPTicket ticket)
    {
        this.ticket = ticket;
    }

    protected override bool VerifySignature(ISigner signer)
    {
        Span<byte> ticketBody = this.ticket.Data.AsSpan();
        ticketBody = ticketBody.Slice(this.ticket.BodyHeader.Position, this.ticket.BodyHeader.Length + 4);
        signer.BlockUpdate(ticketBody);

        return signer.VerifySignature(TrimSignature(this.ticket.TicketSignature));
    }
}