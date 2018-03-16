using System;
using System.Collections.Generic;
using System.Security.Cryptography.Pkcs;
using System.Text;

namespace FreieWahl.Security.Signing.Buergerkarte
{
    public class SignatureHandler
    {
        public SignedCms DecodeSignedData(string data)
        {
            var rawData = Convert.FromBase64String(data);
            SignedCms cms = new SignedCms();
            cms.Decode(rawData);
            return cms;
        }
    }
}
