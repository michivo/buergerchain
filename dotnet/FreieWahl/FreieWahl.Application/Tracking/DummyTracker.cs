using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FreieWahl.Application.Tracking
{
    public class DummyTracker : ITracker
    {
        public Task Track(string path, string clientId)
        {
            return Task.CompletedTask;
        }
    }
}