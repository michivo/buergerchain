using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;

namespace FreieWahl.Application.VotingResults
{
    /// <summary>
    /// Handles storage and retrieval of voting results
    /// </summary>
    public interface IVotingResultManager
    {
        /// <summary>
        /// Stores a given vote
        /// </summary>
        /// <param name="votingId">the voting id of the voting this vote was cast in</param>
        /// <param name="questionIndex">the index of the question this vote was cast for</param>
        /// <param name="answers">a list of answer ids (may also be empty)</param>
        /// <param name="token">the voting token that is used for casting this vote</param>
        /// <param name="signedToken">the signed voting token - only if the voting token and the signed token match will the vote be stored</param>
        /// <returns>the future of this operation</returns>
        Task StoreVote(string votingId, int questionIndex, List<string> answers, string token, string signedToken);

        /// <summary>
        /// gets all results for a given voting id for a list of tokens - a voter may use this data to check if all their votes were accounted for properly
        /// </summary>
        /// <param name="votingId">the id of the voting</param>
        /// <param name="tokens">the voting tokens</param>
        /// <returns>a list of votes, all cast in the voting with the given id using the given voting tokens</returns>
        Task<IReadOnlyCollection<Vote>> GetResults(string votingId, string[] tokens);

        /// <summary>
        /// gets all votes for a given voting
        /// </summary>
        /// <param name="votingId">a voting id</param>
        /// <returns>a list of all votes cast in the voting with the given id</returns>
        Task<IReadOnlyCollection<Vote>> GetResults(string votingId);

        /// <summary>
        /// gets all votes for a given voting id and the given question
        /// </summary>
        /// <param name="votingId">a voting id</param>
        /// <param name="questionIndex">a question index</param>
        /// <returns>a list of all votes cast in the voting with the given id and the question with the given index</returns>
        Task<IReadOnlyCollection<Vote>> GetResults(string votingId, int questionIndex);
    }
}