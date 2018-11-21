using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace FreieWahl.Application.Tracking
{
    public class GoogleAnalyticsTracker : ITracker
    {
        private readonly string _trackingId;
        private readonly HttpClient _client;

        public GoogleAnalyticsTracker(string trackingId)
        {
            _trackingId = trackingId;
            _client = new HttpClient();
        }

        public Task Track(string path, string clientId)
        {
            try
            {
                string anonCid = clientId;
                if (anonCid.Length > 3)
                {
                    anonCid = anonCid.Substring(0, anonCid.Length - 2);
                }
                string postData = $"v=1&tid={_trackingId}&cid={HttpUtility.UrlEncode(anonCid)}&uip={HttpUtility.UrlEncode(clientId)}&dp={HttpUtility.UrlEncode(path)}&t=pageview";

                return _client.PostAsync("https://www.google-analytics.com/collect", new StringContent(postData));
            }
            catch (Exception)
            {
                return Task.CompletedTask;
            }
        }
    }
}