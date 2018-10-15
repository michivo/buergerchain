﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IVotingStore _votingStore;
        private readonly SHA256Managed _hasher;
        private readonly Dictionary<long, Dictionary<int, Vote>> _lastVoteCache;
        private readonly Dictionary<long, Dictionary<int, SemaphoreSlim>> _voteStoreLocks;

        public VotingResultManager(ITimestampService timestampService,
            IVotingTokenHandler votingTokenHandler, IVotingResultStore votingResultStore,
            IVotingChainBuilder votingChainBuilder,
            IVotingStore votingStore)
        {
            _timestampService = timestampService;
            _votingTokenHandler = votingTokenHandler;
            _votingResultStore = votingResultStore;
            _votingChainBuilder = votingChainBuilder;
            _votingStore = votingStore;
            _hasher = new SHA256Managed();
            _lastVoteCache = new Dictionary<long, Dictionary<int, Vote>>();
            _voteStoreLocks = new Dictionary<long, Dictionary<int, SemaphoreSlim>>();
        }

        public async Task StoreVote(long votingId, int questionIndex, List<string> answers, string token, string signedToken)
        {
            if (_votingTokenHandler.Verify(signedToken, token, votingId, questionIndex) == false)
                throw new InvalidOperationException("Token and signature do not match");
            var data = _GetRawData(votingId, questionIndex, answers, token, signedToken);
            var timeStamp = await _timestampService.GetToken(data);
            var timeStampString = _TokenToString(timeStamp);
            var semaphore = _GetSemaphore(votingId, questionIndex);
            await semaphore.WaitAsync();
            try
            {
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
                _lastVoteCache[votingId][questionIndex] = newVote;
                await _votingResultStore.StoreVote(newVote).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private SemaphoreSlim _GetSemaphore(long votingId, int questionIndex)
        {
            if(!_voteStoreLocks.ContainsKey(votingId))
                _voteStoreLocks.Add(votingId, new Dictionary<int, SemaphoreSlim>());

            if(!_voteStoreLocks[votingId].ContainsKey(questionIndex))
                _voteStoreLocks[votingId][questionIndex] = new SemaphoreSlim(1, 1);

            return _voteStoreLocks[votingId][questionIndex];
        }

        private static string _TokenToString(TimeStampToken timeStamp)
        {
            var cms = timeStamp.ToCmsSignedData();
            var enc = (CmsProcessableByteArray) cms.SignedContent;
            var stream = enc.GetInputStream();
            byte[] encData;
            using (var bs = new BinaryReader(stream))
            {
                encData = bs.ReadBytes((int) stream.Length);
            }

            return Convert.ToBase64String(encData);
        }

        private async Task<string> _GetLastVoteSignature(long votingId, int questionIndex)
        {
            if (_lastVoteCache.ContainsKey(votingId) == false)
                _lastVoteCache.Add(votingId, new Dictionary<int, Vote>());

            if (_lastVoteCache[votingId].ContainsKey(questionIndex))
                return _votingChainBuilder.GetSignature(_lastVoteCache[votingId][questionIndex]);

            var lastVote = await _votingResultStore.GetLastVote(votingId, questionIndex);
            if (lastVote != null)
            {
                return _votingChainBuilder.GetSignature(lastVote);
            }

            var vote = await _votingStore.GetById(votingId);
            var question = vote.Questions[questionIndex];
            return _votingChainBuilder.GetGenesisValue(question);
        }

        public void CompleteVoting(long votingId)
        {
            _lastVoteCache.Remove(votingId);
        }

        public void CompleteQuestion(long votingId, int questionIndex)
        {
            if (_lastVoteCache.ContainsKey(votingId))
                _lastVoteCache[votingId].Remove(questionIndex);
        }

        public async Task<IReadOnlyCollection<Vote>> GetResults(long votingId, string[] tokens)
        {
            var allVotes = await _votingResultStore.GetVotes(votingId).ConfigureAwait(false);
            return allVotes.Where(x => tokens.Contains(x.Token)).ToList();
        }

        private byte[] _GetRawData(long votingId, int questionIndex, List<string> answers, string token, string signedToken)
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(1)); // version
            result.AddRange(BitConverter.GetBytes(votingId)); // voting id
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
