using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FreieWahl.Application.Registrations
{
    public class RemoteTokenStore : IRemoteTokenStore
    {
        private readonly string _remoteUrl;

        public RemoteTokenStore(string remoteUrl)
        {
            _remoteUrl = remoteUrl;
        }

        public async Task<string> GrantRegistration(string registrationStoreId, string signedChallengeString,
            List<string> signedTokens)
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
                    tokens = signedTokens
                };

                var jsonObject = new JObject(resultData);
                var json = jsonObject.ToString(Formatting.None);

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
            var request = WebRequest.CreateHttp(_remoteUrl + "getChallenge");
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
    }
}