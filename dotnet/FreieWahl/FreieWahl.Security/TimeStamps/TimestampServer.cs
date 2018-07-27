using System.IO;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;

namespace FreieWahl.Security.TimeStamps
{
    public class TimestampServer
    {
        public TimestampServer(string url, string certPem)
        {
            Url = url;
            var pemReader = new PemReader(new StringReader(certPem));
            Certificate = (X509Certificate) pemReader.ReadObject();
        }

        public TimestampServer(string url, X509Certificate certificate)
        {
            Url = url;
            Certificate = certificate;
        }

        public string Url { get; }

        public X509Certificate Certificate { get; }
    }
}
