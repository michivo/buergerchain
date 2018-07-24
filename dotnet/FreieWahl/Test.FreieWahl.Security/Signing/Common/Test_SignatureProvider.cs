using System;
using System.Text;
using System.IO;
using FreieWahl.Security.Signing.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace Test.FreieWahl.Security.Signing.Common
{
    /// <summary>
    /// Summary description for Test_SignatureProvider
    /// </summary>
    [TestClass]
    public class Test_SignatureProvider
    {
        #region KEY
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
#endregion


        [TestMethod]
        public void TestSimpleSigning()
        {
            // arrange
            var pemReader = new PemReader(new StringReader(_key));
            var key = (AsymmetricCipherKeyPair) pemReader.ReadObject();
            var sigProvider = new SignatureProvider(key);

            var data = Guid.NewGuid().ToByteArray();

            // act
            var signedData = sigProvider.SignData(data);

            // assert
            Assert.IsTrue(sigProvider.IsSignatureValid(data, signedData));
            data[0] = data[0] != Byte.MaxValue ? Byte.MaxValue : Byte.MinValue;
            Assert.IsFalse(sigProvider.IsSignatureValid(data, signedData));
        }

        [TestMethod]
        public void TestMoreSigning()
        {
            // arrange
            var pemReader = new PemReader(new StringReader(_key));
            var key = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            var sigProvider = new SignatureProvider(key);

            var data = Encoding.UTF8.GetBytes(_key);

            // act
            var signedData = sigProvider.SignData(data);

            // assert
            Assert.IsTrue(sigProvider.IsSignatureValid(data, signedData));
            data[0] = data[0] != Byte.MaxValue ? Byte.MaxValue : Byte.MinValue;
            Assert.IsFalse(sigProvider.IsSignatureValid(data, signedData));
        }
    }
}
