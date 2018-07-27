using System.IO;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;

namespace FreieWahl.Security.TimeStamps
{
    public class TimestampServer
    {
        public TimestampServer(string url, string certPem, int priority)
        {
            Url = url;
            Priority = priority;
            var pemReader = new PemReader(new StringReader(certPem));
            Certificate = (X509Certificate) pemReader.ReadObject();
        }

        public TimestampServer(string url, X509Certificate certificate, int priority)
        {
            Url = url;
            Certificate = certificate;
            Priority = priority;
        }

        public int Priority
        {
            get => _priority;
            set
            {
                if (value > 100)
                {
                    _priority = 100;
                }
                else
                {
                    _priority = value < 0 ? 0 : value;
                }
            }
        }

        private int _priority;

        public string Url { get; }

        public X509Certificate Certificate { get; }
    }
}
