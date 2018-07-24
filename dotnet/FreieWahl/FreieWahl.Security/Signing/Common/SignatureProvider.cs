using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace FreieWahl.Security.Signing.Common
{
    public class SignatureProvider : ISignatureProvider
    {
        private readonly ISigner _signer;
        private readonly ISigner _verifier;

        public SignatureProvider(AsymmetricCipherKeyPair key)
        {
            _signer = SignerUtilities.GetSigner("SHA256withRSA");
            _verifier = SignerUtilities.GetSigner("SHA256withRSA");

            _signer.Init(true, key.Private);
            _verifier.Init(false, key.Public);
        }

        public byte[] SignData(byte[] data)
        {
            _signer.Reset();
            _signer.BlockUpdate(data, 0, data.Length);
            return _signer.GenerateSignature();
        }

        public bool IsSignatureValid(byte[] data, byte[] signature)
        {
            _verifier.BlockUpdate(data, 0, data.Length);
            return _verifier.VerifySignature(signature);
        }
    }
}