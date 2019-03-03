using System.Threading.Tasks;

namespace FreieWahl.Voting.Registrations
{
    /// <summary>
    /// Stores challenges required for authenticating registration requests to the tokenstore application
    /// </summary>
    public interface IChallengeStore
    {
        /// <summary>
        /// stores a given challenge
        /// </summary>
        /// <param name="challenge">a challenge</param>
        /// <returns>the future of this operation</returns>
        Task SetChallenge(Challenge challenge);

        /// <summary>
        /// gets the challenge for a given registration
        /// </summary>
        /// <param name="registrationId">the registration id</param>
        /// <returns>the challenge for the registration with the given id</returns>
        Task<Challenge> GetChallenge(string registrationId);

        /// <summary>
        /// deletes the challenge for a given registration id (e.g. when a registration is denied)
        /// </summary>
        /// <param name="registrationId">the registration id</param>
        /// <returns>the future of this operation</returns>
        Task DeleteChallenge(string registrationId);

        /// <summary>
        /// deletes all open challenges for a voting (e.g. when the voting is deleted)
        /// </summary>
        /// <param name="votingId">the id of a voting</param>
        /// <returns>the future of this operation</returns>
        Task DeleteChallenges(string votingId);
    }
}
