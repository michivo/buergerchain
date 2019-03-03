using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Registrations;

namespace FreieWahl.Application.Registrations
{
    /// <summary>
    /// Creates a challenge for authentication via mobile phones/SMS
    /// </summary>
    public interface IChallengeHandler
    {
        /// <summary>
        /// creates, sends and stores a new challenge
        /// </summary>
        /// <param name="recipientName">the recipient's name</param>
        /// <param name="recipientAddress">the address (mail address, e-mail address, or phone number) of the recipient</param>
        /// <param name="voting">the voting the voter has registered for</param>
        /// <param name="registrationId">the registration id</param>
        /// <param name="challengeType">the challenge type (mail, e-mail, sms)</param>
        /// <returns>the future of this operation</returns>
        Task CreateChallenge(string recipientName, string recipientAddress, StandardVoting voting, string registrationId, ChallengeType challengeType);

        /// <summary>
        /// gets the challenge for a given registration id. this is called when the user provides a reply for the challenge
        /// </summary>
        /// <param name="registrationId">the registration id</param>
        /// <returns>the challenge sent out to the voter with the given registration id</returns>
        Task<Challenge> GetChallengeForRegistration(string registrationId);
    }
}
