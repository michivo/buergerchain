using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FreieWahl.Security.TimeStamps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Tsp;

namespace Test.FreieWahl.Security.TimeStamps
{
    /// <summary>
    /// Summary description for Test_TimestampService
    /// </summary>
    [TestClass]
    public class Test_TimestampService
    {
        // List of free RFC3161 servers: https://gist.github.com/Manouchehri/fd754e402d98430243455713efada710
        #region Certificates

        private static string _certificate = @"-----BEGIN CERTIFICATE-----
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
MIIE3DCCA8SgAwIBAgIRAP5n5PFaJOPGDVR8oCDCdnAwDQYJKoZIhvcNAQELBQAw
fjELMAkGA1UEBhMCUEwxIjAgBgNVBAoTGVVuaXpldG8gVGVjaG5vbG9naWVzIFMu
QS4xJzAlBgNVBAsTHkNlcnR1bSBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTEiMCAG
A1UEAxMZQ2VydHVtIFRydXN0ZWQgTmV0d29yayBDQTAeFw0xNjAzMDgxMzEwNDNa
Fw0yNzA1MzAxMzEwNDNaMHcxCzAJBgNVBAYTAlBMMSIwIAYDVQQKDBlVbml6ZXRv
IFRlY2hub2xvZ2llcyBTLkEuMScwJQYDVQQLDB5DZXJ0dW0gQ2VydGlmaWNhdGlv
biBBdXRob3JpdHkxGzAZBgNVBAMMEkNlcnR1bSBFViBUU0EgU0hBMjCCASIwDQYJ
KoZIhvcNAQEBBQADggEPADCCAQoCggEBAL9Xi7yRM1ouVzF/JVf0W1NYaiWq6IEg
zA0dRzhwGqMWN523RHS1GoEk+vUYSjhLC6C6xb80b+qM9Z1CGtAxqFbdqCUOtDwl
xazGy1zjgJLqo68tAEBAfNJBKB8rCOhR0F2JcCJsaXbQdhI8LksHKSbp+AHh0OUo
9iTDFfqmkIR0hVyDLA7E2nhJlGodJIaX6SLAxgw14HQyqj27Adh+zBNMIMeVLUn2
8S0XvMYp9/hVdpx9Fdze4UKVk2CZ90PFlEIhvZisHLNm3P14YEQ/PcSVaWfuYcva
0LnmdvehPwT00+dxryECXhHaU6SmtZF42ZARW7Sh7qduCtlzpDgFUiMCAwEAAaOC
AVowggFWMAwGA1UdEwEB/wQCMAAwHQYDVR0OBBYEFPM1yo5GCA05jd9BxzNuZOQW
O5grMB8GA1UdIwQYMBaAFAh2zcsH/yT2xc3tu5C84oQ3RnX3MA4GA1UdDwEB/wQE
AwIHgDAWBgNVHSUBAf8EDDAKBggrBgEFBQcDCDAvBgNVHR8EKDAmMCSgIqAghh5o
dHRwOi8vY3JsLmNlcnR1bS5wbC9jdG5jYS5jcmwwawYIKwYBBQUHAQEEXzBdMCgG
CCsGAQUFBzABhhxodHRwOi8vc3ViY2Eub2NzcC1jZXJ0dW0uY29tMDEGCCsGAQUF
BzAChiVodHRwOi8vcmVwb3NpdG9yeS5jZXJ0dW0ucGwvY3RuY2EuY2VyMEAGA1Ud
IAQ5MDcwNQYLKoRoAYb2dwIFAQswJjAkBggrBgEFBQcCARYYaHR0cDovL3d3dy5j
ZXJ0dW0ucGwvQ1BTMA0GCSqGSIb3DQEBCwUAA4IBAQDKdOQ4vTLJGjz6K1jFVy01
UwuQ3i0FsvEzMkAblv8iRYc5rgzwGc7B0DJEGjMMgOs9Myt8eTROxoFENFhWujkN
8OSzA6w3dcB667dA9pr8foBtqbRViT2YSMpW9FWkLunh0361OJGVxM+7ph51a1ZQ
m26n69Gc4XEg1dWmWKvh5SldgfEEteQbZEKhOHE9e3NkxmnUIjCWsCTDAlsRqDw0
YntnZ+FGhld86IqfkLs4W9m1ieoDKNuNt1sHbTK7h3/cJs4uXujWq9vmptDiGQIS
+aDbPp1SxEy9V4XteO3BlkTNRrDOZdVXcjokxhDhsHPEj1qDrPbGcpT5cnf/AdUh
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

        private static string _symantecCert = @"-----BEGIN CERTIFICATE-----
MIIFODCCBCCgAwIBAgIQewWx1EloUUT3yYnSnBmdEjANBgkqhkiG9w0BAQsFADCB
vTELMAkGA1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMR8wHQYDVQQL
ExZWZXJpU2lnbiBUcnVzdCBOZXR3b3JrMTowOAYDVQQLEzEoYykgMjAwOCBWZXJp
U2lnbiwgSW5jLiAtIEZvciBhdXRob3JpemVkIHVzZSBvbmx5MTgwNgYDVQQDEy9W
ZXJpU2lnbiBVbml2ZXJzYWwgUm9vdCBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTAe
Fw0xNjAxMTIwMDAwMDBaFw0zMTAxMTEyMzU5NTlaMHcxCzAJBgNVBAYTAlVTMR0w
GwYDVQQKExRTeW1hbnRlYyBDb3Jwb3JhdGlvbjEfMB0GA1UECxMWU3ltYW50ZWMg
VHJ1c3QgTmV0d29yazEoMCYGA1UEAxMfU3ltYW50ZWMgU0hBMjU2IFRpbWVTdGFt
cGluZyBDQTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALtZnVlVT52M
cl0agaLrVfOwAa08cawyjwVrhponADKXak3JZBRLKbvC2Sm5Luxjs+HPPwtWkPhi
G37rpgfi3n9ebUA41JEG50F8eRzLy60bv9iVkfPw7mz4rZY5Ln/BJ7h4OcWEpe3t
r4eOzo3HberSmLU6Hx45ncP0mqj0hOHE0XxxxgYptD/kgw0mw3sIPk35CrczSf/K
O9T1sptL4YiZGvXA6TMU1t/HgNuR7v68kldyd/TNqMz+CfWTN76ViGrF3PSxS9TO
6AmRX7WEeTWKeKwZMo8jwTJBG1kOqT6xzPnWK++32OTVHW0ROpL2k8mc40juu1MO
1DaXhnjFoTcCAwEAAaOCAXcwggFzMA4GA1UdDwEB/wQEAwIBBjASBgNVHRMBAf8E
CDAGAQH/AgEAMGYGA1UdIARfMF0wWwYLYIZIAYb4RQEHFwMwTDAjBggrBgEFBQcC
ARYXaHR0cHM6Ly9kLnN5bWNiLmNvbS9jcHMwJQYIKwYBBQUHAgIwGRoXaHR0cHM6
Ly9kLnN5bWNiLmNvbS9ycGEwLgYIKwYBBQUHAQEEIjAgMB4GCCsGAQUFBzABhhJo
dHRwOi8vcy5zeW1jZC5jb20wNgYDVR0fBC8wLTAroCmgJ4YlaHR0cDovL3Muc3lt
Y2IuY29tL3VuaXZlcnNhbC1yb290LmNybDATBgNVHSUEDDAKBggrBgEFBQcDCDAo
BgNVHREEITAfpB0wGzEZMBcGA1UEAxMQVGltZVN0YW1wLTIwNDgtMzAdBgNVHQ4E
FgQUr2PWyqNOhXLgp7xB8ymiOH+AdWIwHwYDVR0jBBgwFoAUtnf6aUhHn1MS1cLq
BzJ2B9GXBxkwDQYJKoZIhvcNAQELBQADggEBAHXqsC3VNBlcMkX+DuHUT6Z4wW/X
6t3cT/OhyIGI96ePFeZAKa3mXfSi2VZkhHEwKt0eYRdmIFYGmBmNXXHy+Je8Cf0c
kUfJ4uiNA/vMkC/WCmxOM+zWtJPITJBjSDlAIcTd1m6JmDy1mJfoqQa3CcmPU1dB
kC/hHk1O3MoQeGxCbvC2xfhhXFL1TvZrjfdKer7zzf0D19n2A6gP41P3CnXsxnUu
qmaFBJm3+AZX4cYO9uiv2uybGB+queM6AL/OipTLAduexzi7D1Kr0eOUA2AKTaD+
J20UMvw/l0Dhv5mJ2+Q5FL3a5NPD6itas5VYVQR9x5rsIwONhSrS/66pYYE=
-----END CERTIFICATE-----
";

        #endregion

        [TestMethod]
        public async Task TestSingleTimestampSymantec()
        {
            var servers = new List<TimestampServer>
            {
                new TimestampServer("http://sha256timestamp.ws.symantec.com/sha256/timestamp", _symantecCert, 20),
            };
            var timeStampService = new TimestampService(servers);
            var data = Encoding.UTF8.GetBytes("Hello World! Hello FreieWahl!");
            var token = await timeStampService.GetToken(data);
            timeStampService.CheckTokenContent(token, data);
        }

        [TestMethod]
        public async Task TestSingleTimestampApple()
        {
            var servers = new List<TimestampServer>
            {
                new TimestampServer("http://timestamp.apple.com/ts01", _appleCert, 20),
            };
            var timeStampService = new TimestampService(servers);
            var data = Encoding.UTF8.GetBytes("Hello World! Hello FreieWahl!");
            var token = await timeStampService.GetToken(data);
            timeStampService.CheckTokenContent(token, data);
        }

        [TestMethod]
        public async Task TestSingleTimestampCertum()
        {
            var servers = new List<TimestampServer>
            {
                new TimestampServer("http://time.certum.pl", _certumCert, 20),
            };
            var timeStampService = new TimestampService(servers);
            var data = Encoding.UTF8.GetBytes("Hello World! Hello FreieWahl!");
            var token = await timeStampService.GetToken(data);
            timeStampService.CheckTokenContent(token, data);
        }

        [TestMethod]
        public async Task TestSingleTimestampFreeTsa()
        {
            var servers = new List<TimestampServer>
            {
                new TimestampServer("https://freetsa.org/tsr", _certificate, 20),
            };
            var timeStampService = new TimestampService(servers);
            var data = Encoding.UTF8.GetBytes("Hello World! Hello FreieWahl!");
            var token = await timeStampService.GetToken(data);
            timeStampService.CheckTokenContent(token, data);
        }


        [TestMethod]
        public async Task TestTimestampIgnoringCert()
        {
            var servers = new List<TimestampServer>
            {
                new TimestampServer("http://timestamp.apple.com/ts01", _certumCert, 80) // bad certificate, but ignored
            };
            var timeStampService = new TimestampService(servers);
            var data = Encoding.UTF8.GetBytes("Hello World! Hello FreieWahl!");
            var token = await timeStampService.GetToken(data, false);
            timeStampService.CheckTokenContent(token, data);
            Assert.AreEqual(servers[0].Priority, 81);
        }

        [TestMethod]
        public async Task TestTimestampWithFallback()
        {
            var servers = new List<TimestampServer>
            {
                new TimestampServer("https://freetsa.org/tsr", _certumCert, 100), // bad certificate!
                new TimestampServer("http://timestamp.apple.com/ts01", _appleCert, 80)
            };
            var timeStampService = new TimestampService(servers);
            var data = Encoding.UTF8.GetBytes("Hello World! Hello FreieWahl!");
            var token = await timeStampService.GetToken(data);
            timeStampService.CheckTokenContent(token, data);
            Assert.AreEqual(servers[0].Priority, 90);
            Assert.AreEqual(servers[1].Priority, 81);
        }

        [TestMethod]
        public async Task TestInvalidCert()
        {
            var servers = new List<TimestampServer>
            {
                new TimestampServer("https://freetsa.org/tsr", _certumCert, 100)
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
                new TimestampServer("https://freetsa.org/tsr", _certificate, 100),
                new TimestampServer("http://timestamp.apple.com/ts01", _appleCert, 100),
                new TimestampServer("http://time.certum.pl", _certumCert, 100)
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
                    timeStampService.CheckTokenContent(token, results[idx]);
                });
            }

            Task.WaitAll(tasks);
        }
    }
}
