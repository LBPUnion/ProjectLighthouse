using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace LBPUnion.ProjectLighthouse.Tickets.Signature;

public abstract class TicketSignatureVerifier
{
    protected abstract ECPublicKeyParameters PublicKey { get; }
    protected abstract string HashAlgorithm { get; }

    protected abstract bool VerifySignature(ISigner signer);

    public bool ValidateSignature()
    {
        ISigner signer = SignerUtilities.GetSigner($"{this.HashAlgorithm}withECDSA");
        signer.Init(false, this.PublicKey);

        return this.VerifySignature(signer);
    }

    protected static ECDomainParameters FromX9EcParams(X9ECParameters param) =>
        new(param.Curve, param.G, param.N, param.H, param.GetSeed());

    // Sometimes psn signatures have one or two extra empty bytes
    // This is slow but it's better than carelessly chopping 0's
    private protected static byte[] TrimSignature(byte[] signature)
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
}