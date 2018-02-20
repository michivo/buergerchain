using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tsp;

namespace FreieWahl.Security.TimeStamps
{
    public class TimestampClient
    {
        private SecureRandom _random;
        private const string Uri = "https://freetsa.org/tsr";
        private const string Sha512Id = "2.16.840.1.101.3.4.2.3";

        public TimestampClient()
        {
            this._random = new SecureRandom();
        }

        public TimeStampToken GetToken(byte[] data)
        {
            var digestAlg = new SHA512Managed();
            var digest = digestAlg.ComputeHash(data);

            var request = _CreateRequest(digest);

            var httpResponse = _GetHttpResponse(request.GetEncoded());
            Console.WriteLine(Encoding.ASCII.GetString(httpResponse));

            var timestampResponse = new TimeStampResponse(httpResponse);

            _ValidateResponse(request, timestampResponse);

            return timestampResponse.TimeStampToken;
        }

        private static void _ValidateResponse(TimeStampRequest request, TimeStampResponse response)
        {
            if (response.Status == 0 || response.Status == 1)
            {
                if (response.TimeStampToken == null)
                {
                    throw new SecurityUtilityException(string.Format(CultureInfo.InvariantCulture, 
                        "Invalid TS response: missing time stamp token {0}", response.Status));
                }

                if (!Equals(response.TimeStampToken.TimeStampInfo.Nonce, request.Nonce))
                {
                    throw new SecurityUtilityException(string.Format(CultureInfo.InvariantCulture, 
                        "Invalid TS response: nonce mismatch {0}", response.Status));
                }

                if (!string.IsNullOrEmpty(request.ReqPolicy) && 0 != string.CompareOrdinal(response.TimeStampToken.TimeStampInfo.Policy, request.ReqPolicy))
                {
                    throw new SecurityUtilityException(string.Format(CultureInfo.InvariantCulture, 
                        "Invalid TS response: policy mismatch {0}", response.Status));
                }

                if (!_CompareImprints(response.TimeStampToken.TimeStampInfo.GetMessageImprintDigest(), request.GetMessageImprintDigest()))
                {
                    throw new SecurityUtilityException(string.Format(CultureInfo.InvariantCulture, 
                        "Invalid TS response: message imprint mismatch {0}", response.Status));
                }
            }
            else
            {
                throw new SecurityUtilityException(string.Format(CultureInfo.InvariantCulture, "Invalid TS response. Response status: {0}", response.Status));
            }
        }

        private static bool _CompareImprints(byte[] imprint1, byte[] imprint2)
        {
            if (imprint1.Length != imprint2.Length)
                return false;

            for (var i = 0; i < imprint1.Length; i++)
            {
                if (imprint1[i] != imprint2[i])
                    return false;
            }

            return true;
        }

        private TimeStampRequest _CreateRequest(byte[] digest)
        {
            TimeStampRequestGenerator tsqGenerator = new TimeStampRequestGenerator();

            tsqGenerator.SetCertReq(false);

            var tsreq = tsqGenerator.Generate(Sha512Id, digest, new BigInteger(64, _random));

            return tsreq;
        }


        private static byte[] _GetHttpResponse(byte[] request)
        {
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(Uri);
            httpReq.Method = "POST";
            httpReq.ContentType = "application/timestamp-query";

            var reqStream = httpReq.GetRequestStream();
            reqStream.Write(request, 0, request.Length);
            reqStream.Close();
            WebResponse httpResp = httpReq.GetResponse();
            var respStream = httpResp.GetResponseStream();

            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = respStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                respStream.Close();
                return ms.ToArray();
            }
        }
    }
}
