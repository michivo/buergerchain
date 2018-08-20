using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
MIIEpAIBAAKCAQEAiLWVeZXCAodbPGprQm0QNKxi1hyIZM1Kd/Hpa1+WC5XIoGib
cYhPOP+H4EtOZFfYQ5Zun0eftV31BY6zYudHI91BGg53rAIG3U3aH7FAg17IBny0
0ApWOPzJWGtlW4ZaFpWtp8nMFaffuTvfqwB44f8FxVg1wuzs+3TqxeK3hf2IPG3l
oRG2414JojsPyLFuJAVCc3CazbYzVHvZ5IRd2M4GBwZT2elNdxsEaulY8jw0osWO
vWR/QDYNRQfIJveaJ2QY3AyDfhlrqUIQX8k6s86/RK3YWCTg04OL1mM2HVPcIpj7
X7SymKXJr8gfeYjdnnGtCGXtv0yuRMwkgtGonQIDAQABAoIBAEI7s30SHMpL2NyW
9Htcyq6yXDo+3af0vCELj5rJVzEqsuZgd/sT3soPJz34TjUYSDtKnTLHKS3eJRE5
t1hyE8ng7qMYokswOjIq/0Q+hLKBD6jw70shzu9SLZyMf7FNkMUDZHx3rMxLJn2g
b/dxxZ88hoe7emSDlDcpyHryIhckZ+U5bWq8m7WnmJsnRa8XGviLC3phzTqirFTB
M0Lt5lsyNwebX06OPja2E7j8hkbc4YCGqyLLUaSMbCjk3iOv3EcTCoBpxjJYzooR
kRnZb+UT3/l0DBuwwb7b1pbU+jL98iH4Ycyufc0Zr+CwCfulNyyMAFmQjkoXYfJM
OrXW8DMCgYEA9Nvc/kFAcManz+KkCeJMb3LRI0ePssglF+aNYzbWagY/eSwKupa2
wt/EsQgtWd5tmwKDrbzcUi3VIA94kzSMpzSvCWOLcrlUQCEFAu23ZPdwpmZlFjVE
JUjaDedluB95IjYHP1qTY0ey9BkkyyPfQWFa3A7DGJ5V6Guk9czsGLsCgYEAju38
VDKz81ny7D+iHEZbfyP3eHk1dUJQu0NhfRt5jy0XYQQyFRzjNQbt8ZPfvWd+4+Mv
BvgedKDnp7okCMeOku5ToqA23lHk4p98daW/JZZ5o3tqkHsRWswlW7XevsE59wDK
SZdGUkZB9Poo1jrYPeuK3tBnleqtUGgkXVDe+ocCgYEAgWJkRh2otW7jYZ/62hYw
GXvsMt4vemLz5ss2zsQ9TEz84UR6btoQvKyNPJZ7kE9OT2hmuDlhmjMJuSUzq2VL
JVdbXnSxTO/NKw3HniKKk9mENwlIRRHkbmcugcZSI5bN5VvzPHDaDM3oK7/Vh1nv
MJcG8d0DDlEsR+IJirC72N0CgYAtJiQU/Mc3UeyYkClHQTpZ2SStG6y1U7No1AoR
mQI3JglCji672Jo0//Fd5FZC4FSG7BbI3svQD5vdscD8PP1ekIY+0tlCNSBWLgcE
qszMtNHLwIqTBS0gP2h1pees3iDPU6KSyIRgLO1c00DfG0t/k84UQETYaH9C7QK/
r3IgNQKBgQCLiiOs8zmyVQAas89H41GYdNYY2eckrglAnP81MYefGVNO9nR+slW3
oYUMxK5gHe4gOoY97uBVDnmKwtSqxxakx/lV4txQ9atV+rNWRXXoL1KzeQzNDnFx
q1BIZdVZcDIj1fF3N3qT9W1FQzTKp4+2gI/dQyAri48uyGxfkpMnWg==
-----END RSA PRIVATE KEY-----";

        [TestMethod]
        public void TestMethod1()
        {
            var pemReader = new PemReader(new StringReader(_key));
            var key = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            RsaKeyParameters publicParams = (RsaKeyParameters)key.Public;
            var publicKey = publicParams.Modulus.ToString(10);
            Console.WriteLine(publicKey);
            var signer = SignerUtilities.GetSigner("SHA256withRSA");

            signer.Init(true, key.Private);
            var rawToken = new BigInteger("726654cb0fe36a80afb8d028ff4feaa718affaf2fcac6d5abee4382a22e242e0ea89b4b0353a03308eab7b69307a493d26e36faa56bce5652ea152e1c57c5fdb89cb069f3814240f49e89b85aa746dac0119cf063137f8eb6a9897dab589abf4d2ac655a0dbe85ecb560c886e77cbf37f809bd2c35c265f54d711dd70707f50e", 16).ToByteArray();
            signer.BlockUpdate(rawToken, 0, rawToken.Length);
            var signedData = signer.GenerateSignature();
            var result = new BigInteger(signedData);
            Assert.IsTrue(!string.IsNullOrEmpty(result.ToString(16)));
        }
    }
}
