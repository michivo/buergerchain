using System;
using System.Security.Cryptography.Pkcs;
using System.Text;
using Org.BouncyCastle.Security;

namespace FreieWahl.Security.Signing.Buergerkarte
{
    public class SignatureHandler
    {
        private SignedCms _DecodeSignedData(string data)
        {
            var rawData = Convert.FromBase64String(data);
            SignedCms cms = new SignedCms();
            cms.Decode(rawData);
            return cms;
        }

        public void VerifySignature(string data, string signedData)
        {
            var cms = _DecodeSignedData(signedData);
            cms.CheckSignature(false);
            var contentData = Encoding.UTF8.GetString(cms.ContentInfo.Content);
            if(!data.Equals(contentData, StringComparison.OrdinalIgnoreCase))
                throw new SignatureException("Invalid signature");
        }
    }
}
