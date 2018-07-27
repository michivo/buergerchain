using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FreieWahl.Security.TimeStamps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;

namespace Test.FreieWahl.Security.TimeStamps
{
    /// <summary>
    /// Summary description for Test_TimestampService
    /// </summary>
    [TestClass]
    public class Test_TimestampService
    {
        #region Certificate
        static string _certificate = @"-----BEGIN CERTIFICATE-----
MIIIATCCBemgAwIBAgIJAMHphhYNqOmCMA0GCSqGSIb3DQEBDQUAMIGVMREwDwYD
VQQKEwhGcmVlIFRTQTEQMA4GA1UECxMHUm9vdCBDQTEYMBYGA1UEAxMPd3d3LmZy
ZWV0c2Eub3JnMSIwIAYJKoZIhvcNAQkBFhNidXNpbGV6YXNAZ21haWwuY29tMRIw
EAYDVQQHEwlXdWVyemJ1cmcxDzANBgNVBAgTBkJheWVybjELMAkGA1UEBhMCREUw
HhcNMTYwMzEzMDE1NzM5WhcNMjYwMzExMDE1NzM5WjCCAQkxETAPBgNVBAoTCEZy
ZWUgVFNBMQwwCgYDVQQLEwNUU0ExdjB0BgNVBA0TbVRoaXMgY2VydGlmaWNhdGUg
ZGlnaXRhbGx5IHNpZ25zIGRvY3VtZW50cyBhbmQgdGltZSBzdGFtcCByZXF1ZXN0
cyBtYWRlIHVzaW5nIHRoZSBmcmVldHNhLm9yZyBvbmxpbmUgc2VydmljZXMxGDAW
BgNVBAMTD3d3dy5mcmVldHNhLm9yZzEiMCAGCSqGSIb3DQEJARYTYnVzaWxlemFz
QGdtYWlsLmNvbTESMBAGA1UEBxMJV3VlcnpidXJnMQswCQYDVQQGEwJERTEPMA0G
A1UECBMGQmF5ZXJuMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAtZEE
jE5IbzTp3Ahif8I3UWIjaYS4LLEwvv9RfPw4+EvOXGWodNqyYhrgvOfjNWPg7ek0
/V+IIxWfB4SICCJ0YMHtiCYXBvQoEzQ1nfu4G9E1P8F5YQrxqMjIZdwA6iOzqJvm
vQO6hansgn1gVlkF4i1qWE7ROArhUCgM7jl+mKAS84BGQAeGJEO8B3y5X0Ia8xcS
2Wg8223/uvPIululZq5SPUWdYXc0bU2EDieIa3wBxbiQ14ouJ7uo3S+aKBLhV9Yv
khxlliVIBp3Nt9Bt4YHeDpVw1m+HIgzii2KKtVkG8+4MIQ9wUej0hYr4uaktCeRq
8tnLpb/PrRaM32BEkaSwZgOxFMr3Ax8GXn7u+lPFdfNJDAWdLjLdx2rE1MTHEGg7
l/0b5ZG8YQVRhtiPmgORswe2+R7ZVNqjb5rNah4Uqi5K3xdGS1TbGNu2/+MAgCRl
RzcENs5Od7rl3m/g8/nW5/++tGHnlOkvsJUfiq5hpBLM6bIQdGNci+MnrhoPa0pk
brD4RjvGO/hFUwQ10Z6AJRHsn2bDSWlS2L7LabCqTUxB9gUV/n3LuJMZzdpZumrq
S+POrnGOb8tszX25/FC7FbEvNmWwqjByicLm3UsRHOSLotnv21prmlBgaTNPs09v
x64zDws0IIqsgN8yZv3ZBGWHa6LLiY2VBTFbbnsCAwEAAaOCAdswggHXMAkGA1Ud
EwQCMAAwHQYDVR0OBBYEFG52C3tOT5zhYMptLOknoqKUs3c3MB8GA1UdIwQYMBaA
FPpVDYw0ZlFDTPfns6dsla965qSXMAsGA1UdDwQEAwIGwDAWBgNVHSUBAf8EDDAK
BggrBgEFBQcDCDBjBggrBgEFBQcBAQRXMFUwKgYIKwYBBQUHMAKGHmh0dHA6Ly93
d3cuZnJlZXRzYS5vcmcvdHNhLmNydDAnBggrBgEFBQcwAYYbaHR0cDovL3d3dy5m
cmVldHNhLm9yZzoyNTYwMDcGA1UdHwQwMC4wLKAqoCiGJmh0dHA6Ly93d3cuZnJl
ZXRzYS5vcmcvY3JsL3Jvb3RfY2EuY3JsMIHGBgNVHSAEgb4wgbswgbgGAQAwgbIw
MwYIKwYBBQUHAgEWJ2h0dHA6Ly93d3cuZnJlZXRzYS5vcmcvZnJlZXRzYV9jcHMu
aHRtbDAyBggrBgEFBQcCARYmaHR0cDovL3d3dy5mcmVldHNhLm9yZy9mcmVldHNh
X2Nwcy5wZGYwRwYIKwYBBQUHAgIwOxo5RnJlZVRTQSB0cnVzdGVkIHRpbWVzdGFt
cGluZyBTb2Z0d2FyZSBhcyBhIFNlcnZpY2UgKFNhYVMpMA0GCSqGSIb3DQEBDQUA
A4ICAQClyUTixvrAoU2TCn/QoLFytB/BSDw+lXxoorzZuXZPGpUBYf1yRy1Bpe7S
d3hiA7VCIkD7OibN4XYIe2+xAR30zBniVxqkoFEQlmXpTEb1C9Kt7mrEE34lGyWj
navaRRUV2P+eByCejsILeHT34aDt58AJN/6EozT4syZc7S2O2d9hOWWDZ3/rOCwe
47I+bqXwXfMN57n4kAXSUmb2EvOci09tq6bXv7rBljK5Bjcyn1Km8GahDkPqqB+E
mmxf4/6LXqIydfaH8gUuUC6mwwdipmjM4Hhx3Y6X4xW7qSniVYmXegoxLOlsUQax
Q3x3nys2GxgoiPPuiiNDdPoGPpVhkmJ/fEMQc5ZdEmCSjroAnoA0Ka4yTPlvBCNU
83vKWv3cefeTRqs4i/x58B3JhhJU6mzBKZQQdrg9IFVvO+UTJoN/KHb3gzs3Dnw9
QQUjgn1PU0AMciGNdSKf8QxviJOpo6HAxCu0yJjBPfQcf2VztPxWUVlxphCnsNKF
fIIlqfsgTqzsouiXGqGvh4hqKuPHL+CgquhCmAp3vvFrkhFUWAkNmCtZRmA3ZOda
CtPRFFS5mG9ni5q2r+hJcDOuOr/U60O3vJ3uaIFZSeZIFYKoLnhSd/IoIQfv45Ag
DgUIrLjqguolBSdvPJ2io9O0rTi7+IQr2jb8JEgpH1WNwC3R4A==
-----END CERTIFICATE-----
";

        private static string _certumCert = @"-----BEGIN CERTIFICATE-----
MIIElTCCA32gAwIBAgIUJ+aS4Kljd4Fu23Hp++0y+ElpFbswDQYJKoZIhvcNAQEF
BQAwbjELMAkGA1UEBhMCUEwxLjAsBgNVBAoMJU1pbmlzdGVyIHdsYXNjaXd5IGRv
IHNwcmF3IGdvc3BvZGFya2kxLzAtBgNVBAMMJk5hcm9kb3dlIENlbnRydW0gQ2Vy
dHlmaWthY2ppIChOQ0NlcnQpMB4XDTE2MDQwMTE0MzkzN1oXDTIwMTAyNjIzNTk1
OVowXTEVMBMGA1UEBRMMTnIgd3Bpc3U6IDE1MQswCQYDVQQGEwJQTDEhMB8GA1UE
CgwYQXNzZWNvIERhdGEgU3lzdGVtcyBTLkEuMRQwEgYDVQQDDAtDRVJUVU0gUVRT
QTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAN2nFu94S1vAFuPhXSNy
NUQJgAl/5z/avS7Fx3DMMo5wtYAR0L3w9zfU0miFqWNIZP/0iiTbyGe/PnEn7O95
ydGhsPb1abS6pQ3rU20tc3YqdkhI3uUQEm4kesANGHxZtCbRup8F+rnxILBT55/0
v2DIu2xv6zad5oLivXcqUwjudeAEJQKmGgtf8XDRBj0hkXqsQW6uzn1s0l6YuetS
KQVyUs9A3XcdifqQt4lyZIlVjJSqUopzJjL9WbwUnCR/nh2J6vqMcuO5KdI1Kkvx
O8k4uUca0KoZAODqlgM5tGQRZHWg+V19umP0DrFZe20vrLl0bpeTBfQoeuERt3CW
jKMCAwEAAaOCATowggE2MBYGA1UdJQEB/wQMMAoGCCsGAQUFBwMIMAwGA1UdEwEB
/wQCMAAwgasGA1UdIwSBozCBoIAUWTQM+33nRQFvyXCWwk4G+A+BQ/ahcqRwMG4x
CzAJBgNVBAYTAlBMMS4wLAYDVQQKDCVNaW5pc3RlciB3bGFzY2l3eSBkbyBzcHJh
dyBnb3Nwb2RhcmtpMS8wLQYDVQQDDCZOYXJvZG93ZSBDZW50cnVtIENlcnR5Zmlr
YWNqaSAoTkNDZXJ0KYIUYqcNBMMkuNQnVsw/gWvy6zLvBxkwMQYDVR0gAQH/BCcw
JTAjBgRVHSAAMBswGQYIKwYBBQUHAgEWDXd3dy5uY2NlcnQucGwwDgYDVR0PAQH/
BAQDAgbAMB0GA1UdDgQWBBStXdivePhtlTJL/rHXlK86nM4VhzANBgkqhkiG9w0B
AQUFAAOCAQEARqcpOhHLJMgR3Ih7k5CqSWPL15mkk39IcKhGl2Hk6mhPV+jVix75
7/S6g4myYwKNLS5IrqyT6xOM3nw+wv3pFfMQbJEPE5BN+L7L219tImCxpP4P/nnp
5Gv5VGZZLp2KbPaTX4j9KdH9qMOnAi6ZEsmCDUD1Xb2udAmM1JlKiyJIURbTmFsP
uYS/hDDQ/ITkw2ju2grevXWNR31I0MkaVqnVPmeioDBLGab0eBiCuOWwta5xK1Of
xTYYWbtaO24RnUhZ8obLvxlzDv89kGFTf1K598UabNGDqBbtf8ylMfYV0YnoBCgZ
IxVNtBHRQVDi2QSTedpiYWpDvSm+s0aE2Q==
-----END CERTIFICATE-----
";

        private static string _appleCert = @"-----BEGIN CERTIFICATE-----
MIIEBzCCAu+gAwIBAgIIfUxXY5/z8LcwDQYJKoZIhvcNAQELBQAwYjELMAkGA1UE
BhMCVVMxEzARBgNVBAoTCkFwcGxlIEluYy4xJjAkBgNVBAsTHUFwcGxlIENlcnRp
ZmljYXRpb24gQXV0aG9yaXR5MRYwFAYDVQQDEw1BcHBsZSBSb290IENBMB4XDTEy
MDQwNTEyMDI0NFoXDTI3MDQwNTEyMDI0NFowfDEwMC4GA1UEAwwnQXBwbGUgVGlt
ZXN0YW1wIENlcnRpZmljYXRpb24gQXV0aG9yaXR5MSYwJAYDVQQLDB1BcHBsZSBD
ZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTETMBEGA1UECgwKQXBwbGUgSW5jLjELMAkG
A1UEBhMCVVMwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDTdxih95kQ
Z1zSLp64jyNnPvxC4gl9Coq4GPxzQC+9xNhQxSfI/rg0cKANEzy9CE6ak285N9qe
ZfW0Y/SQyEltXSDTOf0JuvQ6885KaWQFmUbg2jXEZRgexhajEmG0LvXwiQ2M3D32
Bs9vhiVMCcIbyA54iI3BIri6IRObyu6Knt17W/+j6dGjgX7+/+aMSeQ7CvkQpnIz
uyzESlpyCjlQdN0obnlffqeoFM9Ws1ZspenwxK756iCOGMcodOIITYkmQnle9mDj
RVih+1FJXpJKTbnv1HO12gR741Kfy6MZXaxrmGye4ux0LUQ+4GE+B0V+NHUmmECb
dZ7IMO1Lv3ePAgMBAAGjgaYwgaMwHQYDVR0OBBYEFDTNJU7N3jeFOKFYJvj54ine
8hyTMA8GA1UdEwEB/wQFMAMBAf8wHwYDVR0jBBgwFoAUK9BpR5R2Cf70a40uQKb3
R01/CF4wLgYDVR0fBCcwJTAjoCGgH4YdaHR0cDovL2NybC5hcHBsZS5jb20vcm9v
dC5jcmwwDgYDVR0PAQH/BAQDAgGGMBAGCiqGSIb3Y2QGAgkEAgUAMA0GCSqGSIb3
DQEBCwUAA4IBAQA20vXecVMHySPYeJtlvPPVW+m4fxsjx6LPtKko6fjdcIghOfPb
M5zDckPWPUJRl7qtHY6S0nWLw12c9cuM3GpqOt3rVH3tFGvz1j6TyG16VF/yQ44Q
0HZcmwAMHU7KPM365vfCPnK3uN7oNKoVoK5cZ6gMrJseZbPjDzBCNOmu0wHTp91C
c3V8UUOFmmAQ3K4n0mtnyTNFb8mYHqCaf00Rk+Fp/+xLRfNOyiIOV9ciB+UitIfp
nNNFy24/5Y64/EbVXMmwqwU6bTcoo6hGZW9VoWiI6lI+yfTU5vo/pOQmgLU6a9bD
5fkygcgyokjhjgajGeSzyztL3+DMDrKvmNGD
-----END CERTIFICATE-----
";

#endregion

        [TestMethod]
        public async Task TestSingleTimestamp()
        {
            var servers = new List<TimestampServer>
            {
                //new TimestampServer("https://freetsa.org/tsr", _certificate)
                new TimestampServer("http://timestamp.apple.com/ts01", _appleCert),
            };
            var timeStampService = new TimestampService(servers);
            var data = Encoding.UTF8.GetBytes("Hello World! Hello FreieWahl!");
            var token = await timeStampService.GetToken(data);
            var cmsData = token.ToCmsSignedData();
            var signers = cmsData.GetSignerInfos();
            //var certs2 = cmsData.GetCertificates("*");
            //var foo = cmsData.GetCrls(null);
            //Console.WriteLine(foo.ToString());
            var certs3 = cmsData.GetCertificates("Collection");
            Console.WriteLine(certs3.ToString());
            Console.WriteLine(signers.Count);
            var swriter = new StringWriter();
            var writer = new PemWriter(swriter);
            writer.WriteObject(token.SignerID.Certificate);
            var s = swriter.ToString();
            Console.WriteLine(s);
            Assert.IsTrue(_CheckTokenContent(token, data));
        }

        private static bool _CheckTokenContent(TimeStampToken token, byte[] data)
        {
            var cms = token.ToCmsSignedData();
            var enc = (CmsProcessableByteArray) cms.SignedContent;
            var stream = enc.GetInputStream();
            byte[] encData;
            using (var bs = new BinaryReader(stream))
            {
                encData = bs.ReadBytes((int) stream.Length);
            }

            Asn1InputStream asnis = new Asn1InputStream(encData);

            var asnobj = asnis.ReadObject();
            Asn1StreamParser parser = new Asn1StreamParser(asnis);
            Asn1Sequence seq = Asn1Sequence.GetInstance(asnobj);
            foreach (Asn1Encodable asn1Encodable in seq)
            {
                if (asn1Encodable is Asn1Sequence subseq)
                {
                    if (subseq.Count != 2)
                        continue;
                    if (subseq[0] is Asn1Sequence hashSeq)
                    {
                        if (hashSeq.Count > 1 && hashSeq[0] is DerObjectIdentifier objId &&
                            objId.Id == "2.16.840.1.101.3.4.2.3")
                        {
                            var encoded = subseq[1] as DerOctetString;
                            if (encoded == null)
                                continue;

                            var readDigest = encoded.GetOctets();
                            var digestAlg = new SHA512Managed();
                            var digest = digestAlg.ComputeHash(data);
                            Assert.AreEqual(readDigest.Length, digest.Length);
                            for (int i = 0; i < digest.Length; i++)
                            {
                                Assert.AreEqual(readDigest[i], digest[i]);
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [TestMethod]
        public async Task TestInvalidCert()
        {
            var servers = new List<TimestampServer>
            {
                new TimestampServer("https://freetsa.org/tsr", _certumCert)
            };
            var timeStampService = new TimestampService(servers);
            var data = Encoding.UTF8.GetBytes("Hello World! Hello FreieWahl!");
            await Assert.ThrowsExceptionAsync<TspValidationException>(async () =>
            {
                var token = await timeStampService.GetToken(data);
            });
        }

        [TestMethod]
        public void TestTenTimestamps()
        {
            var servers = new List<TimestampServer>
            {
                //new TimestampServer("https://freetsa.org/tsr", _certificate),
                new TimestampServer("http://timestamp.apple.com/ts01", _appleCert),
                new TimestampServer("http://time.certum.pl", _certumCert)
            };
            var timeStampService = new TimestampService(servers);

            List<byte[]> results = new List<byte[]>();
            for (int i = 0; i < 10; i++)
            {
                var data = Encoding.UTF8.GetBytes("Hello World! Hello FreieWahl!" + i);
                results.Add(data);
            }

            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                var idx = i;
                tasks[i] = Task.Run(async () =>
                {
                    var token = await timeStampService.GetToken(results[idx]).ConfigureAwait(false);
                    Assert.IsTrue(_CheckTokenContent(token, results[idx]));
                });
            }

            Task.WaitAll(tasks);
        }
    }
}
