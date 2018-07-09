using System.Security.Cryptography.X509Certificates;

namespace FreieWahl.Security.Signing.Buergerkarte
{
    public class SignedData
    {
        public SignedData(string signeeName, string signeeId, X509Certificate2 rootCertificate, string data)
        {
            SigneeName = signeeName;
            SigneeId = signeeId;
            RootCertificate = rootCertificate;
            Data = data;
        }
        
        public string SigneeName { get; }

        public string SigneeId { get; }

        public X509Certificate2 RootCertificate { get; }

        public string Data { get; }
    }
}
