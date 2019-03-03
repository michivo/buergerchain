using System.Collections.Generic;
using FreieWahl.Voting.Models;

namespace FreieWahl.Application.VotingResults
{
    /// <summary>
    /// builds a block chain like data structure for the voting results.
    /// For each question in each voting, one block chain is created.
    /// </summary>
    public interface IVotingChainBuilder
    {
        /// <summary>
        /// Gets the "genesis" value, i.e. the signature for the first block. The signature for the
        /// first block is calculated based on the question a vote belongs to, so a change in the question
        /// can be detected later on.
        /// </summary>
        /// <param name="q">a question</param>
        /// <returns>a digest for the given question (default implementation uses Keccak for the digest)</returns>
        string GetGenesisValue(Question q);

        /// <summary>
        /// the signature for a given vote - this is used for all further blocks after the first one.
        /// </summary>
        /// <param name="v">a vote</param>
        /// <returns>a digest/signature for the given question (default implementation uses Keccak for the digest)</returns>
        string GetSignature(Vote v);

        /// <summary>
        /// checks the block chain for a given question - throws an exception if the block chain is broken
        /// </summary>
        /// <param name="q">a question</param>
        /// <param name="votes">a list of votes that were cast for this question</param>
        void CheckChain(Question q, IReadOnlyList<Vote> votes);
    }
}
