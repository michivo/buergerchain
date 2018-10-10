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
            string signedChallengeString, List<string> signedTokens, string votingUrl)
        {
            var request = WebRequest.CreateHttp(_remoteUrl + "grantRegistration");
            request.ContentType = "application/json";
            request.Method = WebRequestMethods.Http.Post;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                var resultData = new
                {
                    registrationId = registrationStoreId,
                    challengeSignature = signedChallengeString,
                    tokens = signedTokens,
                    votingId = voting.Id.ToString(CultureInfo.InvariantCulture),
                    startDate = voting.StartDate.ToString("HH:mm") + ", " + voting.StartDate.ToString("dd.MM.yyyy"),
                    endDate = voting.EndDate.ToString("HH:mm") + ", " + voting.EndDate.ToString("dd.MM.yyyy"),
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

        public async Task InsertPublicKeys(long votingIdVal, IEnumerable<RsaKeyParameters> publicKeys)
        {
            var request = WebRequest.CreateHttp(_remoteUrl + "setKeys");
            request.ContentType = "application/json";
            request.Method = WebRequestMethods.Http.Post;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                var publicKeyStructures = publicKeys as RsaKeyParameters[] ?? publicKeys.ToArray();
                var resultData = new
                {
                    votingId = votingIdVal.ToString(CultureInfo.InvariantCulture),
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