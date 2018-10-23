using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FreieWahl.Application.Authentication
{
    public class SessionCookieProvider : ISessionCookieProvider
    {
        private readonly string _url;

        public SessionCookieProvider(string url)
        {
            _url = url;
        }

        public async Task<SessionCookie> CreateSessionCookie(string idToken)
        {
            var request = _CreateRequest(idToken);

            var httpResponse = await request.GetResponseAsync();
            string result;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            { // TODO: error handling
                result = streamReader.ReadToEnd();
            }

            var jResult = JObject.Parse(result);
            var token = jResult["session"].ToString();
            var expires = int.Parse(jResult["maxAge"].ToString());
            var maxAge = DateTimeOffset.UtcNow.AddMilliseconds(expires);
            return new SessionCookie(token, maxAge);
        }

        private HttpWebRequest _CreateRequest(string idToken)
        {
            var request = WebRequest.CreateHttp(_url);
            request.ContentType = "application/json";
            request.Method = WebRequestMethods.Http.Post;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                var resultData = new
                {
                    idToken
                };

                var json = JsonConvert.SerializeObject(resultData);

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            return request;
        }
    }
}
