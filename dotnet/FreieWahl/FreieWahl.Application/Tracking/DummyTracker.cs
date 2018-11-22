using System.Threading.Tasks;

namespace FreieWahl.Application.Tracking
{
    public class DummyTracker : ITracker
    {
        public Task Track(string path, string clientAddress, string userAgent)
        {
            return Task.CompletedTask;
        }

        public Task TrackSpending(string votingId, string cost)
        {
            return Task.CompletedTask;
        }
    }
}