using System;
using System.Text;
using System.Collections.Generic;
using FreieWahl.Security.Signing.VotingTokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Test.FreieWahl.Security.Signing.VotingTokens
{
    /// <summary>
    /// Summary description for Test_VotingTokenSigning
    /// </summary>
    [TestClass]
    public class Test_VotingTokenSigning
    {
        private static VotingTokenSigning _signer;
        private static VotingTokenVerifier _verifier;

        #region Additional test attributes

        [ClassInitialize()]
        public static void TokenSigningInitialize(TestContext testContext)
        {
            var keyGenParams = new RsaKeyGenerationParameters(
                new BigInteger("65537"), new SecureRandom(), 2048, 5);
            var keyGen = new RsaKeyPairGenerator();
            keyGen.Init(keyGenParams);
            var keys = keyGen.GenerateKeyPair();
            _signer = new VotingTokenSigning(keys);
            _verifier = new VotingTokenVerifier(keys);
        }
                
        #endregion

        [TestMethod]
        public void TestMethodSignAndVerify()
        {
            var message1 = "Hello world!";

            var signed = _signer.Sign(message1);
            Assert.IsTrue(_verifier.Verify(signed, message1));
        }
    }
}
