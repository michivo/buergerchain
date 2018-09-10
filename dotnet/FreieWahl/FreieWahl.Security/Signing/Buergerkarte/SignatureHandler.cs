using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Cms;

namespace FreieWahl.Security.Signing.Buergerkarte
{
    public class SignatureHandler : ISignatureHandler
    {
        private readonly X509Certificate2[] _trustedRootCertificates;
        //static DerObjectIdentifier sFullNameIdentifier = new DerId

        public SignatureHandler(X509Certificate2[] trustedRootCertificates)
        {
            _trustedRootCertificates = trustedRootCertificates;
        }

        private CmsSignedData _DecodeSignedData(string data)
        {
            var rawData = Convert.FromBase64String(data);
            CmsSignedData cms = new CmsSignedData(rawData);
            return cms;
        }

        public SignedData GetSignedContent(string signedData)
        {
            var cms = _DecodeSignedData(signedData);
            var enc = (CmsProcessableByteArray)cms.SignedContent;
            var stream = enc.GetInputStream();
            string content;
            using (var reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }
            var signers = cms.GetSignerInfos().GetSigners().Cast<SignerInformation>().ToList();

            if(signers.Count > 1)
                throw new Exception("CMS should be signed by exactly one signer");

            var certs = cms.GetCertificates("Collection");
            var matches = certs.GetMatches(signers[0].SignerID);
            var signerCert = matches.Cast<Org.BouncyCastle.X509.X509Certificate>().Single();
            var fullName = signerCert.SubjectDN.GetValueList(new DerObjectIdentifier("2.5.4.3")).Cast<string>().Single(); // id-at-commonName
            var serial = signerCert.SubjectDN.GetValueList(new DerObjectIdentifier("2.5.4.5")).Cast<string>().Single(); // id-at-serialNumber

            // var leafCert = new X509Certificate2(signerCert.GetEncoded()); // checking cert is easier with .net
            // var rootIssuer = _CheckIssuer(leafCert);
            // var root = _GetRootCertificate(rootIssuer); // TODO - does not work in app engine, cert chain misses cert

            return new SignedData(fullName, serial, null, content);
        }

        private static X509Certificate2 _CheckIssuer(X509Certificate2 leafCert)
        {
            if (leafCert.Subject == leafCert.Issuer)
            {
                return leafCert;
            }

            X509Chain chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.Build(leafCert);
            X509Certificate2 issuer = chain.ChainElements[chain.ChainElements.Count - 1].Certificate;
            chain.Reset();
            return issuer;
        }

        private X509Certificate2 _GetRootCertificate(X509Certificate2 cmsCertificate)
        {
            cmsCertificate.Verify();
            var issuer = _CheckIssuer(cmsCertificate);
            foreach (var rootCertificate in _trustedRootCertificates)
            {
                if (rootCertificate.Equals(issuer))
                    return issuer;
            }

            throw new Exception("Invalid root certificate");
        }
    }
}
