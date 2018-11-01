using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FreieWahl.Security.Signing.VotingTokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Test.FreieWahl.Security.Signing.VotingTokens
{
    /// <summary>
    /// Summary description for Test_VotingKeyStore
    /// </summary>
    [TestClass]
    public class Test_VotingKeyStore
    {
        private static readonly string ProjectId = "freiewahl-data";
        private VotingKeyStore _votingStore;

        [TestInitialize]
        public void Init()
        {
            _votingStore = new VotingKeyStore(ProjectId, VotingKeyStore.TestNamespace);
            //_votingStore.ClearAll();
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public async Task TestWriteAndRead()
        {
            var keyGenParams = new RsaKeyGenerationParameters(
                new BigInteger("65537"), new SecureRandom(), 2048, 5);
            var keyGen = new RsaKeyPairGenerator();
            keyGen.Init(keyGenParams);
            var keys1 = keyGen.GenerateKeyPair();
            var keys2 = keyGen.GenerateKeyPair();
            
            var keyDict = new Dictionary<int, AsymmetricCipherKeyPair>()
            {
                {1, keys1 },
                {2, keys2 }
            };

            await _votingStore.StoreKeyPairs("1234", keyDict);

            var readKeys1 = _votingStore.GetKeyPair("1234", 1);
            var readKeys2 = _votingStore.GetKeyPair("1234", 2);
            var private1 = (RsaPrivateCrtKeyParameters) readKeys1.Private;
            var private2 = (RsaPrivateCrtKeyParameters) readKeys2.Private;

            var origPrivate1 = (RsaPrivateCrtKeyParameters)keys1.Private;
            var origPrivate2 = (RsaPrivateCrtKeyParameters)keys2.Private;
            Assert.AreEqual(origPrivate1, private1);
            Assert.AreEqual(origPrivate2, private2);
            Assert.AreNotEqual(origPrivate2, private1);
            Assert.AreNotEqual(origPrivate1, private2);
        }
    }
}
