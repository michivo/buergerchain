using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;

namespace FreieWahl.Application.Registrations
{
    public class RemoteTokenStore : IRemoteTokenStore
    {
        private readonly string _remoteUrl;

        public RemoteTokenStore(string remoteUrl)
        {
            _remoteUrl = remoteUrl;
        }

        public async Task<string> GrantRegistration(string registrationStoreId, StandardVoting voting,
            string signedChallengeString, List<string> signedTokens, string votingUrl, TimeSpan utcOffset, string timezoneName)
        {
            var request = WebRequest.CreateHttp(_remoteUrl + "grantRegistration");
            request.ContentType = "application/json";
            request.Method = WebRequestMethods.Http.Post;
            var startTime = voting.StartDate.Subtract(utcOffset);
            var endTime = voting.EndDate.Subtract(utcOffset);
            var timezoneInfo = string.IsNullOrEmpty(timezoneName) ? string.Empty : " (Zeitzone: " + timezoneName + ")";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                var resultData = new
                {
                    registrationId = registrationStoreId,
                    challengeSignature = signedChallengeString,
                    tokens = signedTokens,
                    votingId = voting.Id.ToString(CultureInfo.InvariantCulture),
                    startDate = startTime.ToString("HH:mm") + ", " + startTime.ToString("dd.MM.yyyy"),
                    endDate = endTime.ToString("HH:mm") + ", " + endTime.ToString("dd.MM.yyyy") + timezoneInfo,
                    votingTitle = voting.Title,
                    link = votingUrl
                };

                var json = JsonConvert.SerializeObject(resultData);

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = await request.GetResponseAsync();
            string result;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public async Task<RegistrationChallenge> GetChallenge(string registrationStoreId)
        {
            var request = WebRequest.CreateHttp(_remoteUrl + "getChallengeAndTokens");
            request.ContentType = "application/json";
            request.Method = WebRequestMethods.Http.Post;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{\"registrationId\":\"" + registrationStoreId + "\" }";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = await request.GetResponseAsync();
            string result;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            var jsonResult = JObject.Parse(result);
            var challenge = (string) jsonResult["challenge"];
            var tokens = (JArray) jsonResult["tokens"];

            return new RegistrationChallenge(challenge,
                tokens.Select(x => (string)x).ToList());
        }

        public async Task InsertPublicKeys(string votingIdVal, IEnumerable<RsaKeyParameters> publicKeys)
        {
            var request = WebRequest.CreateHttp(_remoteUrl + "setKeys");
            request.ContentType = "application/json";
            request.Method = WebRequestMethods.Http.Post;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                var publicKeyStructures = publicKeys as RsaKeyParameters[] ?? publicKeys.ToArray();
                var resultData = new
                {
                    votingId = votingIdVal,
                    exponents = publicKeyStructures.Select(x => x.Exponent.ToString(16)).ToArray(),
                    moduli = publicKeyStructures.Select(x => x.Modulus.ToString(16)).ToArray()
                };

                var json = JsonConvert.SerializeObject(resultData);

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            await request.GetResponseAsync();
        }
    }
}