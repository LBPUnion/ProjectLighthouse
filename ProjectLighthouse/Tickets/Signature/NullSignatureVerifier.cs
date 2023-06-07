using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace LBPUnion.ProjectLighthouse.Tickets.Signature;

public class NullSignatureVerifier : TicketSignatureVerifier
{
    protected override ECPublicKeyParameters PublicKey => null;
    protected override string HashAlgorithm => null;
    protected override bool VerifySignature(ISigner signer) => false;
}