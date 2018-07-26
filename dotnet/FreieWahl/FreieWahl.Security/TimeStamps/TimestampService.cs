using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FreieWahl.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;

namespace FreieWahl.Security.TimeStamps
{
    public class TimestampService : ITimestampService
    {
        private readonly string _serverUrl;
        private readonly SecureRandom _random;
        private readonly ILogger _logger;
        private X509Certificate _cert;
        private const string Sha512Id = "2.16.840.1.101.3.4.2.3"; // sha512NoSign according to https://msdn.microsoft.com/en-us/library/ff635603.aspx
        private const string RequestContentType = "application/timestamp-query";
        private const string RequestMethod = "POST";

        public TimestampService(string serverUrl, string certificate, ILogger logger = null)
        {
            PemReader reader = new PemReader(new StringReader(certificate));
            var obj = reader.ReadObject();
            _cert = (X509Certificate)obj;
            _serverUrl = serverUrl;
            _logger = logger ?? NullLogger.Instance;
            _random = new SecureRandom();
        }

        public async Task<TimeStampToken> GetToken(byte[] data)
        {
            try
            {
                _logger.LogTrace("Got new request for token with {0} bytes of data", data.Length);
                var digestAlg = new SHA512Managed();
                var digest = digestAlg.ComputeHash(data);

                var request = _CreateRequest(digest);
                _logger.LogTrace("Sending http request to server {0}", _serverUrl);
                var httpResponse = await _GetHttpResponse(request.GetEncoded()).ConfigureAwait(false);
                _logger.LogTrace("Received response from server");
                var timestampResponse = new TimeStampResponse(httpResponse);
                _logger.LogTrace("Successfully converted response for time stamp request");
                _ValidateResponse(request, timestampResponse);
                _logger.LogDebug("Successfully converted and validated response for time stamp request");
                timestampResponse.TimeStampToken.Validate(_cert);
                return timestampResponse.TimeStampToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing time stamp token request");
                throw;
            }
        }

        private static void _ValidateResponse(TimeStampRequest request, TimeStampResponse response)
        {
            if (response.Status == (int)PkiStatus.Granted || response.Status == (int)PkiStatus.GrantedWithMods)
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

                if (!string.IsNullOrEmpty(request.ReqPolicy) && 
                    !response.TimeStampToken.TimeStampInfo.Policy.Equals(request.ReqPolicy, StringComparison.OrdinalIgnoreCase))
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


        private async Task<byte[]> _GetHttpResponse(byte[] request)
        {
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(_serverUrl);
            httpReq.Method = RequestMethod;
            httpReq.ContentType = RequestContentType;

            var reqStream = httpReq.GetRequestStream();
            reqStream.Write(request, 0, request.Length);
            reqStream.Close();
            WebResponse httpResp = await httpReq.GetResponseAsync().ConfigureAwait(false);
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
