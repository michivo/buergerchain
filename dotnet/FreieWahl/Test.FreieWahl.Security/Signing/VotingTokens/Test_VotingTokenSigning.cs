using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;

namespace Test.FreieWahl.Security.Signing.VotingTokens
{
    /// <summary>
    /// Summary description for Test_VotingTokenSigning
    /// </summary>
    [TestClass]
    public class Test_VotingTokenSigning
    {
        static string _key = @"-----BEGIN RSA PRIVATE KEY-----
MIIEogIBAAKCAQEAi78Obk3hSU9F/N4pd2fkJr5sCYi8a7f67KBhuuPqgp/KDFGz
u/cJqjvwnumcqfdRpUmprtmHsH0gSbnN0IsUHBFe1vd1wOHQaCmvvuZYrqYTHahQ
4HHFqwADd2mTmuKWveXUxCr3+ZIZ+1mANa9d3y0YPHPqRgZe1z8IeNNBD+pfkWZg
65UAW93SCrmJ5xGEoFn0iW0eJxx4cuFkG6UAmt/fezMM4V4FRCLWd+vwN/rJNXua
plXlf1yaUgUb/6eHr1vKaGq8b98FUgmhXIkGk5xwzzqwArSojKDv/7PY9h6Yzadv
v1z9jCruE/H3CYpk4AiNara+qFcPkHfiWwuhQQIDAQABAoIBAC/9eo0yvY0ZZiWC
fab9zIHpntxfUqWDVu5v3V/66QvWp+I3JIwGOBF36BgKjbxorGcgtt7O4SQgtDfd
UOqu+EbzSCh2br/ATHuY5TufixDfA22J4cxSBdnFdWtKnF/yNGYeSUTcpK/WFooU
oEzPXmNH7yzDec52XZd5lAU6NvuptPZta+jdligsHuf9h8JSIcbgPIRPife8G/eD
7p4uJftP/KM8ikmp/yFekye6uo565CQ4Na/5c4rk/lK5O8t37HSHT5frqssHvzmO
+tX0Xf9s3IMlwCP6VmSf0zke0O16fb8Qo3+NrbVRsY8tFsLE5wGC5fmJ30wA/a8+
cGNQqKkCgYEAxz1iOiIROa+hakf4WSqyjhGwYhd7LQEemMoheCylIRf4toJ0Zcn7
49LpYNuXA07NWcgiD1bRIgmAFdiLWTLUTsx2GJUpsH/2+hy4vXJbT+k1z7OSakXe
/XTfY7qiwid2vOmXdVSkKLjKzJhjYgcpwoPVXu/j8xXZQZWFcw3Xd/8CgYEAs47L
6TN3AIE2qQFJvFQQ1xsj7PtZ8xFPwKIV0nnDPm2nwf5Zt/gZe4PExXU6CEPFsMYX
b2rrNb/5H1sze1vU4kBJW9dEM2B2+dXm/KfTzt+aKFqIrnTlM7cdHcma1jICY1LZ
GZqv8de6O0VKxD0kq0vGoHj1rvRsuSlsPV6G5r8CgYActr8CS8iZvLMRox+qkhm7
mdcGvAWXfdLQCEl8jUqbE15Xx0NJLCvuMIZL3DnOUzwLFWm7NjLbqezYuSWUDFxG
ovbeIhkpA7gvYZZKT0HTqXOE5IdUY494jbBoKgys2I0nOq8GTNV/vOoVRF0GUqv1
CZTlosMCVILEDe12oGcONwKBgEG7rPqaVJ5ir1f/mLbRL7kPvn8rJSrO05t5uvNq
kAdyrU9fhgjLUsjc3FRNLgmIhf7JBhtL0P/EX7cS9C3c2Hpbfn+Ytub4EZY0sHMe
HXRGd3D+c7sqIB5mksos2/o+JJGn6sJs1qx6hTrdixl8KMhs3TIyFf8OjB7dAzsU
N96RAoGAK/dF+6wNIxlRSSpCZjiJ0umHMEsj3yjQOMwIpBc3eljPsoPtvJydqyVU
EU5tE7H42g/EW5KiDovY4vKJPrrBwB7JcSP1fc8EcP06JhBa0pN4IZLJQ+B3r6UV
GSa3OfeGgyo6YdlaD7qr9x+ltyyYqX60n/eZrKYQPzKdOQ4ImUg=
-----END RSA PRIVATE KEY-----";

        [TestMethod]
        public void TestMethod1()
        {
            var pemReader = new PemReader(new StringReader(_key));
            var key = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            RsaKeyParameters publicParams = (RsaKeyParameters)key.Public;
            RsaPrivateCrtKeyParameters privateKey = (RsaPrivateCrtKeyParameters) key.Private;
            var pubN = publicParams.Modulus.ToString(10);

            //signer.Init(true, key.Private);
            var blindedToken = new BigInteger("5cf1ebac13c1246effcddc5296aca1741114ebb11ab082d369c9edf6ee6b8d3e952af8f54fa725ec4d7653196fd690e6bd51e0182f1c79d3efad7a9dab29a8b968091504d636a2a7a9261923e3f793a59b302d0770c79b06f0ccb1cbbaf68fb44c9d9368e673fb9a520a8794cb9ac3dddf2e45dca5fa4420cf081c613da20bbbe5a679dd17e5fb036c7fa8a3ed115134abffeb5f5da1a8b09261712f5e7ab459c61e4ee1cd599d722e968293dc479ef967e5788a44844644cbfa8414e2ca71dea4fcc924a85d9748c67c781f0fed4389680b68283422a3b3a1b21b97fc96e21e6e721c5d511c2f340e31024c013769faf0a6e4098d3c58183323317c87a1623c", 16);
            var signed2 = blindedToken.ModPow(privateKey.Exponent, publicParams.Modulus);
            var signed = signed2.ToString(16);
            Console.WriteLine(signed);

            var unblindedToken = new BigInteger("44327549afe519c0840a1da8ad698078b1e97a4c0a8e17887fc65f33b63ec1aab757d9dddb20503777fb0cd66105d2e9eb9a7e8e20b3479ffc090dd3845e03b0d348c2f6a5123710156f759c0206950bd7ea398dd44bbe31064e3c8516bebf3b121e4355051d82e35ff2149e824cbbb02c412cffd6c481b94d71a0bdd94431a7909b187183d1150a6d06aea88398e1d3fef2fc8a7c431d537c8821d203c19bc29cdc9dcb14396f7b38f6f0976e62984ad5424d7ac183f6d41eb3d35623b2fd22153ecd607c4001f1d9c2a72e1e368d55426fb7d0a5fa55a3960a88a6bac2df99a11c3c10b897026756cbdfeff0d8a10120230d7948872fb44533fcee31fd7a23", 16);
            var origToken = "dcfd22e3-f858-43bb-847a-0abaedfb649d";

            var publicKey = (RsaKeyParameters)key.Public;
            var digest = new SHA256Managed();

            var hashRaw = digest.ComputeHash(Encoding.UTF8.GetBytes(origToken)).SkipWhile(x => x == 0).ToArray();
            var sigIntVerification = unblindedToken.ModPow(publicKey.Exponent, publicKey.Modulus);
            var sigUsRaw = sigIntVerification.ToByteArray().SkipWhile(x => x == 0).ToArray();
            Assert.AreEqual(hashRaw.Length, sigUsRaw.Length);
            for (int i = 0; i < hashRaw.Length; i++)
            {
                Assert.AreEqual(sigUsRaw[i], hashRaw[i]);
            }
        }
    }
}





