using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;

namespace FreieWahl.Voting.Storage
{
    /// <summary>
    /// stores votes (i.e. voting results) in a block chain like data structure
    /// </summary>
    public interface IVotingResultStore
    {
        /// <summary>
        /// stores a vote
        /// </summary>
        /// <param name="v">the vote that should be stored</param>
        /// <param name="getBlockSignature">callback for getting the signature for a block</param>
        /// <param name="getGenesisSignature">callback for generating the genesis block signature</param>
        /// <returns>the future of this operation</returns>
        Task StoreVote(Vote v, Func<Vote, string> getBlockSignature, Func<Task<string>> getGenesisSignature);

        /// <summary>
        /// gets all votes for a given voting id
        /// </summary>
        /// <param name="votingId">the voting id</param>
        /// <returns>a list for all votes for all questions with the given voting id</returns>
        Task<IReadOnlyCollection<Vote>> GetVotes(string votingId);

        /// <summary>
        /// gets all votes for a given voting id and question index
        /// </summary>
        /// <param name="votingId">the voting id</param>
        /// <param name="questionIndex">the index of a question</param>
        /// <returns>a list for all votes for a given question in the voting with the given id</returns>
        Task<IReadOnlyCollection<Vote>> GetVotes(string votingId, int questionIndex);
        
        /// <summary>
        /// gets the last vote for a given voting with for the question with the given index
        /// </summary>
        /// <param name="votingId">the voting id</param>
        /// <param name="questionIndex">the question index</param>
        /// <returns>the last vote for the voting with the given id for the question with the given index</returns>
        Task<Vote> GetLastVote(string votingId, int questionIndex);
    }
}