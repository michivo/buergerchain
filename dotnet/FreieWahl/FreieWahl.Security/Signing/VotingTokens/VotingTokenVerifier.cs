using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public class VotingTokenVerifier : IVotingTokenVerifier
    {
        private ISigner _verifier;

        public VotingTokenVerifier(AsymmetricCipherKeyPair keyPair)
        {
            _verifier = SignerUtilities.GetSigner("SHA256withRSA");
            _verifier.Init(false, keyPair.Public);
        }

        public bool Verify(string signature, string origMessage)
        {
            var sigRaw = new BigInteger(signature, 16);
            var sigRawData = sigRaw.ToByteArray();
            var messageRaw = Encoding.UTF8.GetBytes(origMessage);
            _verifier.BlockUpdate(messageRaw, 0, messageRaw.Length);
            return _verifier.VerifySignature(sigRawData);
        }
    }
}