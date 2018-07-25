using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    public class RemoteTokenStore : IRemoteTokenStore
    {
        private readonly string _remoteUrl;

        public RemoteTokenStore(string remoteUrl)
        {
            _remoteUrl = remoteUrl;
        }

        public async Task<string> GrantRegistration(long registrationId, string signedChallengeString)
        {
            var request = WebRequest.CreateHttp(_remoteUrl);
            request.ContentType = "application/json";
            request.Method = WebRequestMethods.Http.Post;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{\"registrationId\":\"" + registrationId.ToString(CultureInfo.InvariantCulture) + "\"," +
                                 "\"signedChallenge\":\"" + signedChallengeString + "\"}";

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
    }
}