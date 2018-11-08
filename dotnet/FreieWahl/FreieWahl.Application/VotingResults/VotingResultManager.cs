using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FreieWahl.Application.Voting;
using FreieWahl.Security.Signing.VotingTokens;
using FreieWahl.Security.TimeStamps;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Tsp;

namespace FreieWahl.Application.VotingResults
{
    public class VotingResultManager : IVotingResultManager
    {
        private readonly ITimestampService _timestampService;
        private readonly IVotingTokenHandler _votingTokenHandler;
        private readonly IVotingResultStore _votingResultStore;
        private readonly IVotingChainBuilder _votingChainBuilder;
        private readonly IVotingManager _votingManager;
        private readonly SHA256Managed _hasher;

        public VotingResultManager(ITimestampService timestampService,
            IVotingTokenHandler votingTokenHandler, IVotingResultStore votingResultStore,
            IVotingChainBuilder votingChainBuilder,
            IVotingManager votingManager)
        {
            _timestampService = timestampService;
            _votingTokenHandler = votingTokenHandler;
            _votingResultStore = votingResultStore;
            _votingChainBuilder = votingChainBuilder;
            _votingManager = votingManager;
            _hasher = new SHA256Managed();
        }

        public async Task StoreVote(string votingId, int questionIndex, List<string> answers, string token, string signedToken)
        {
            if (await _votingTokenHandler.Verify(signedToken, token, votingId, questionIndex) == false)
                throw new InvalidOperationException("Token and signature do not match");

            var data = _GetRawData(votingId, questionIndex, answers, token, signedToken);
            var timeStamp = await _timestampService.GetToken(data);
            var timeStampString = _TokenToString(timeStamp);

            var lastBlockSignature = await _GetLastVoteSignature(votingId, questionIndex);
            var newVote = new Vote
            {
                VotingId = votingId,
                QuestionIndex = questionIndex,
                SelectedAnswerIds = answers,
                PreviousBlockSignature = lastBlockSignature,
                SignedToken = signedToken,
                Token = token,
                TimestampData = timeStampString
            };

            await _votingResultStore.StoreVote(newVote, _votingChainBuilder.GetSignature, 
                _GetGenesisSignature(votingId, questionIndex)).ConfigureAwait(false);

        }

        private Func<Task<string>> _GetGenesisSignature(string votingId, int questionIndex)
        {
            return async () =>
            {
                var vote = await _votingManager.GetById(votingId);
                var question = vote.Questions.Single(x => x.QuestionIndex == questionIndex);
                return _votingChainBuilder.GetGenesisValue(question);
            };
        }

        private static string _TokenToString(TimeStampToken timeStamp)
        {
            var cms = timeStamp.ToCmsSignedData();
            var enc = (CmsProcessableByteArray)cms.SignedContent;
            var stream = enc.GetInputStream();
            byte[] encData;
            using (var bs = new BinaryReader(stream))
            {
                encData = bs.ReadBytes((int)stream.Length);
            }

            return Convert.ToBase64String(encData);
        }

        private async Task<string> _GetLastVoteSignature(string votingId, int questionIndex)
        {
            var lastVote = await _votingResultStore.GetLastVote(votingId, questionIndex);
            if (lastVote != null)
            {
                return _votingChainBuilder.GetSignature(lastVote);
            }

            var vote = await _votingManager.GetById(votingId);
            var question = vote.Questions.Single(x => x.QuestionIndex == questionIndex);
            return _votingChainBuilder.GetGenesisValue(question);
        }

        public Task<IReadOnlyCollection<Vote>> GetResults(string votingId)
        {
            return _votingResultStore.GetVotes(votingId);
        }

        public Task<IReadOnlyCollection<Vote>> GetResults(string votingId, int questionIndex)
        {
            return _votingResultStore.GetVotes(votingId, questionIndex);
        }

        public async Task<IReadOnlyCollection<Vote>> GetResults(string votingId, string[] tokens)
        {
            var allVotes = await _votingResultStore.GetVotes(votingId).ConfigureAwait(false);
            return allVotes.Where(x => tokens.Contains(x.Token)).ToList();
        }

        private byte[] _GetRawData(string votingId, int questionIndex, List<string> answers, string token, string signedToken)
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(2)); // version
            var idBytes = Encoding.ASCII.GetBytes(votingId);
            result.AddRange(BitConverter.GetBytes(idBytes.Length));
            result.AddRange(idBytes);
            result.AddRange(BitConverter.GetBytes(questionIndex)); // question index
            result.AddRange(BitConverter.GetBytes(answers.Count)); // # answers
            foreach (var answer in answers)
            {
                result.Add(2);
                result.AddRange(Encoding.ASCII.GetBytes(answer)); // answer id
                result.Add(3);
            }
            result.Add(2); // separator
            result.AddRange(Encoding.ASCII.GetBytes(token)); // token, in our application only consists of 0-9, a-f and -
            result.Add(2);
            result.AddRange(Encoding.ASCII.GetBytes(signedToken)); // signed token, in our application only consists of 0-9, a-f
            var hash = _hasher.ComputeHash(result.ToArray());
            result.AddRange(hash);
            return result.ToArray();
        }
    }
}
