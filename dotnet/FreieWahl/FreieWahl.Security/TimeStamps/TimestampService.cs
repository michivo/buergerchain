using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;

namespace FreieWahl.Security.TimeStamps
{
    public class TimestampService : ITimestampService
    {
        private readonly SecureRandom _random;
        private readonly ILogger _logger;
        private List<TimestampServer> _servers;
        private const string Sha512Id = "2.16.840.1.101.3.4.2.3"; // sha512NoSign according to https://msdn.microsoft.com/en-us/library/ff635603.aspx
        private const string RequestContentType = "application/timestamp-query";
        private const string RequestMethod = "POST";
        private int _serverIndex;
        private const int BadServerMalus = 10;
        private const int GoodServerBonus = 1;

        public TimestampService(List<TimestampServer> tsServers, ILogger logger = null)
        {
            if (tsServers.Count == 0)
                throw new ArgumentException("At least one timestamp server is required");

            _serverIndex = 0;
            _servers = tsServers;

            _logger = logger ?? NullLogger.Instance;
            _random = new SecureRandom();
        }

        public async Task<TimeStampToken> GetToken(byte[] data, bool checkCertificate = true)
        {
            var servers = _servers.OrderByDescending(x => x.Priority);
            Exception lastException = null;

            foreach (var server in servers)
            {
                try
                {
                    var token = await _GetTokenWithServer(data, checkCertificate, server);
                    server.Priority += GoodServerBonus;
                    return token;
                }
                catch (Exception ex)
                {
                    server.Priority -= BadServerMalus;
                    _logger.LogDebug("Failed getting timestamp from server with url " + server.Url + ", setting prio to " + server.Priority);
                    lastException = ex;
                }
            }

            throw lastException ?? new TspException("Failed generation time stamp");
        }

        private async Task<TimeStampToken> _GetTokenWithServer(byte[] data, bool checkCertificate, TimestampServer server)
        {
            try
            {
                _logger.LogTrace("Got new request for token with {0} bytes of data", data.Length);
                var digestAlg = new SHA512Managed();
                var digest = digestAlg.ComputeHash(data);

                var request = _CreateRequest(digest, checkCertificate);
                _logger.LogTrace("Sending http request to server {0}", server.Url);
                var httpResponse = await _GetHttpResponse(request.GetEncoded(), server.Url).ConfigureAwait(false);
                _logger.LogTrace("Received response from server");
                var stringa = Encoding.UTF8.GetString(httpResponse);
                Console.WriteLine(stringa);
                var timestampResponse = new TimeStampResponse(httpResponse);
                _logger.LogTrace("Successfully converted response for time stamp request");
                _ValidateResponse(request, timestampResponse);
                _logger.LogDebug("Successfully converted and validated response for time stamp request");

                if (checkCertificate)
                {
                    _CheckCertificate(server.Certificate, timestampResponse);
                }

                return timestampResponse.TimeStampToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing time stamp token request");
                throw;
            }
        }

        private static void _CheckCertificate(X509Certificate serverCertificate, TimeStampResponse timestampResponse)
        {
            var certStore = timestampResponse.TimeStampToken.GetCertificates("Collection");
            var matches = certStore.GetMatches(null).Cast<X509Certificate>();
            bool isValid = false;
            foreach (var x509Certificate in matches)
            {
                if (x509Certificate.Equals(serverCertificate))
                {
                    isValid = true;
                    break;
                }
            }

            if (!isValid)
                throw new TspValidationException("Invalid server certificate");
        }

        public bool CheckTokenContent(TimeStampToken token, byte[] data)
        {
            var cms = token.ToCmsSignedData();
            var enc = (CmsProcessableByteArray)cms.SignedContent;
            var stream = enc.GetInputStream();
            byte[] encData;
            using (var bs = new BinaryReader(stream))
            {
                encData = bs.ReadBytes((int)stream.Length);
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
                            if (digest.Length != readDigest.Length)
                            {
                                throw new TspValidationException("Invalid digest length");
                            }

                            for (int i = 0; i < digest.Length; i++)
                            {
                                if (readDigest[i] != digest[i])
                                {
                                    throw new TspValidationException("Invalid digest");
                                }
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
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

        private TimeStampRequest _CreateRequest(byte[] digest, bool withCertificate)
        {
            TimeStampRequestGenerator tsqGenerator = new TimeStampRequestGenerator();

            tsqGenerator.SetCertReq(withCertificate);

            var tsreq = tsqGenerator.Generate(Sha512Id, digest, new BigInteger(64, _random));

            return tsreq;
        }


        private async Task<byte[]> _GetHttpResponse(byte[] request, string url)
        {
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(url);
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

