using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Test.FreieWahl.Security.Signing.VotingTokens
{
    /// <summary>
    /// Summary description for Test_VotingTokenSigning
    /// </summary>
    [TestClass]
    public class Test_VotingTokenSigning
    {
        static string _key = @"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAss70Mhgyh1mWaNtFw6k07atjidVfPvfR6oxsJgVcxrWTAYhS
5AFOQbppf2JNN8ddkHJftwdLdlN297vDRl9w/IrAbU52ttvRr6UveEx1/d/s9J6k
5Kq1hTWpkA66TZY9cZCzBfSvUWruxHCRTlCr0YI8Epm+fLfduLJa5sfxrJHBkk+W
akeIyaPqjqJxj0Iv7YRpTj07KF7PcGNTkAdW+7BKG8zTYfDeht5NEC/V8vgbCa78
hloscV/NIfb/4n7sAAnxbuRGNbliBkQitYm/LR5opdoLpGUE6jTxPBTXPe5DlXDM
YM7fv9oR18O/OLvPMsNUN/mZ/s/C37/C72qUtwIDAQABAoIBAAK7KH2vYu4wTxzQ
5JLlfbp3mLCdQrQqgtlLRcea41zhnxox49o5ruFQIJZigP1uHR68sHuSL/PhuHp2
MrhbctVYpTHGNgf6+YvuQPhcapzzE6J03d3kQZuEQ0/A+dV/iva2GBXqM9dRg84a
Tg3dK5Kqo5JBKOiswkU07DCEM0vIc42Lx3aK284ZolUoYlJbJZv/PqLzcZxd/PPm
VgePJTpAy3RrQN3YtdmshVg4qCrCtDfX7d4l73ENlE3XTVz36QSPHI62qp77wf7I
uIGoEnl7WGy94UddVxD/xW6SwJ0LfdIwVedf+BnQdknMM18oGwa7Rg0FhD5s+BmY
hdVfIZECgYEA9ErS8THariVpfvVaOgQ41SF371JksUWmzqSH8kGDnzVXdYCluV71
ON0g/bHoWh49j2ZRJELZynYku63ONcBkp8sfM4BD3hTj/T8/U5N8CqUh0jCJw0B7
iQWcQ0Fm4b4q8UlJOeDkbkXGG3G/poHtq+xi22r+ONRsP8UR1Xdf6O8CgYEAu2C4
fLsHPBczZ49habxGfIS7wUlQ2bnm+m5vutgPaKXcnQ4OeiVcZV96jljogNbOwTRV
/ZUiX67CAoNZVCp45TSgzVJPQBDha0nG2D5LDOCFLdYpqP/B/TZ54Uu0DJFh6s2F
aB7D9cCE+agg88uneT8jqQ+WCXXHX6rCiHoYwLkCgYEAyxliIrDGFD56ZNjq+I0G
CvvWUJv5pwA3XFmhxKD/IuAgJEqefW0bBvmhMgo1GKdHmu7/ytvhYdezVm17oWig
xnezKwgaZIqNucBZj8xwNhFv+uXrwu7bReHqNmgrdsa5wPyi6oG0qJFN0QdSxMYE
qQjQb4eWb/z7OlFHMGgczvUCgYBq/a454l0eLa03a8JWqp+gx/WhRyi4OZMu2dJI
YMhjm5ldwEH58s1QQPVsxE12C7Gg1i5njjlDYzj6UF+4VEwVrDhJJL+FuF3OciDt
JpyZ7LV+17OQAQGWgP2U7DIRnw3HEbUkH7UK5PPIzfyK2HV3INtO1Ex6eFrwQEO1
w+nQWQKBgQCuCzMFA2JKDPE5W6Kr11Po4GF+ucC0WZls18uC5+vTKKVHevTcazPY
AkU5y1zRY3ZY+8pOZqyWvp0zRzooG9ptzR+V+byPJSfyafeBoj0A7B3Gf/SqxVJF
XWpyUp0ln/id2ThI8JB5gQaIsqavGeaXKv0VvFO+LUVuS+Rm4b4LMw==
-----END RSA PRIVATE KEY-----";

        [TestMethod]
        public void TestMethod1()
        {
            var pemReader = new PemReader(new StringReader(_key));
            var key = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            RsaKeyParameters publicParams = (RsaKeyParameters)key.Public;
            RsaPrivateCrtKeyParameters privateKey = (RsaPrivateCrtKeyParameters) key.Private;
            var pubN = publicParams.Modulus.ToString(10);
            Console.WriteLine(pubN);

            //signer.Init(true, key.Private);
            var blindedToken = new BigInteger("51b46b81d9dc398df9c6da5bb1d36d7ce35379c6acfb434eb67a6ec408944d87d091d9bc71f601ef11ef73121041a0d896628808d5371b831634ae76ae075696fd1b1060257fc5dcc1daf1b4709abcb10de6ad86fe79ca3afa3d01235431297cdae2d7e4aa54fb5acd3389b5fe6e64ea67ebaf0115b5bc5efae5fca5327ed0ebab3c7e6f338b4f2a231255202a61bf589ef1cc541e1008fa3a5889819012d404b03d9b0c69ad6e3cf3c0d7b6b700f5b61a822b3fe866628a5af2ba9f58c61ea2fcb9d76ce359ba465e00b7a3620bc9fd7412f99707e73162ff986b72b8377ab325c940975c4cf1977cdabb98e50aa0d50ad72311996a33b9533cdcab018ed434", 16);
            var signed2 = blindedToken.ModPow(privateKey.Exponent, publicParams.Modulus);
            var signed = signed2.ToString(16);
            Console.WriteLine(signed);

            var unblindedToken = new BigInteger("8077b5f994ca6094db3eb2bce4b81fb927853aa71e8ed1233833db63142e85b340d70ecec9d5b3d4620ea3d39c071cf42ffeb75dbaf3e4f68786d9d7a63a8b85e079c059e59722418225712f8a3a32f80afe4a1625df93afb9073bc2e794a516b37b8481ec9d67d353497c53d0f66aad84a2758a94be85cfefb686d8bef623abd17a5b4695080bc38cbaf34774c68a2593782d8f3bd6c0d8361962e606412ca0a303c4ec4ff3e928ca56cde319655b2223f852486a9ef20b5b1cdfbfb1910356686c4ae0d4645348f0a35e966b19ab9526091c2962fbed8bd9c1e63002f1b5bfcf0954b0114b73e3f7b177010c2ca984ee97a38933bcacf81055cd97fedb3508", 16);
            var origToken = "6f37aef9-c5d5-429e-a7f3-297a67fbce36";

            var publicKey = (RsaKeyParameters)key.Public;
            var digest = new SHA256Managed();

            var messageHash = new BigInteger(digest.ComputeHash(Encoding.UTF8.GetBytes(origToken)));
            var sigIntVerification = unblindedToken.ModPow(publicKey.Exponent, publicKey.Modulus);
            Assert.AreEqual(messageHash, sigIntVerification);
        }
    }
}





