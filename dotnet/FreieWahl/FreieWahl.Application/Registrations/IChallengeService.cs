using System.Threading.Tasks;
using FreieWahl.Voting.Registrations;

namespace FreieWahl.Application.Registrations
{
    /// <summary>
    /// Handles challenges for users providing proof of id using e.g. SMS
    /// </summary>
    public interface IChallengeService
    {
        /// <summary>
        /// gets a 'standardized' format for the recipient (e.g. phone number 0664/1234567 => +43664124567) as required by the implementer of this service
        /// </summary>
        /// <param name="recipient">the recipient's phone number or address</param>
        /// <returns>the recipient's phone number or address in a standardized format</returns>
        string GetStandardizedRecipient(string recipient);

        /// <summary>
        /// sends a challenge to a registered voter
        /// </summary>
        /// <param name="recipient">the recipient's phone number or address</param>
        /// <param name="votingName">the voting the voter has registered for</param>
        /// <param name="challenge">the challenge that should be sent to the voter</param>
        /// <param name="votingId">the id of the voting the voter has registered for</param>
        /// <returns>the future of this operation</returns>
        Task SendChallenge(string recipient, string challenge, string votingName, string votingId);

        /// <summary>
        /// the challenge type (e.g. SMS) supported by an implementer
        /// </summary>
        ChallengeType SupportedChallengeType { get; }
    }
}
