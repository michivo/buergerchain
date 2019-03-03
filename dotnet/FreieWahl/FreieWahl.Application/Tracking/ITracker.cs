using System;
using System.Threading.Tasks;

namespace FreieWahl.Application.Tracking
{
    /// <summary>
    /// interface for a simple tracker tracking user activity on the site. This is already obsolete, since we are using Google Analytics directly.
    /// </summary>
    public interface ITracker
    {
        /// <summary>
        /// tracks a user actvity
        /// </summary>
        /// <param name="path">the path visited by the user</param>
        /// <param name="clientAddress">the user's ip address</param>
        /// <param name="userAgent">the user's agent (browser) identifier</param>
        /// <returns>the future of this operation</returns>
        Task Track(string path, string clientAddress, string userAgent);

        /// <summary>
        /// tracks all the money that is spent for a given voting (e.g. for sending SMS)
        /// </summary>
        /// <param name="votingId">the id of the voting</param>
        /// <param name="cost">the amount that was spent</param>
        /// <returns>the future of this operation</returns>
        Task TrackSpending(string votingId, string cost);
    }
}
