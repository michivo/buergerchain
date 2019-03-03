using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace FreieWahl.Security.Signing.Common
{
    public class SignatureProvider : ISignatureProvider
    {
        private readonly ISigner _signer;
        private readonly ISigner _verifier;

        // for testing purposes only:
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

        public SignatureProvider(string pemKey)
        {
            var pemReader = new PemReader(new StringReader(pemKey));
            var key = (AsymmetricCipherKeyPair)pemReader.ReadObject();

            _signer = SignerUtilities.GetSigner("SHA256withRSA");
            _verifier = SignerUtilities.GetSigner("SHA256withRSA");

            _signer.Init(true, key.Private);
            _verifier.Init(false, key.Public);
        }

        public SignatureProvider(AsymmetricCipherKeyPair key = null)
        {
            if (key == null)
            {
                var pemReader = new PemReader(new StringReader(_key));
                key = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            }

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