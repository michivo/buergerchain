using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace FreieWahl.Security.Signing.VotingTokens
{
    public class VotingTokenSigning : IVotingTokenSigning
    {
        private readonly ISigner _signer;

        public VotingTokenSigning(AsymmetricCipherKeyPair keyPair)
        {
            _signer = SignerUtilities.GetSigner("SHA256withRSA");
            _signer.Init(true, keyPair.Private);
        }

        public string Sign(string token)
        {
            //_signer.Reset();
            var rawToken = Encoding.UTF8.GetBytes(token);
            _signer.BlockUpdate(rawToken, 0, rawToken.Length);
            var signedData = _signer.GenerateSignature();
            var result = new BigInteger(signedData);
            return result.ToString(16);
        }
    }
}

