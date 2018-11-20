using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Common;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Microsoft.Extensions.Logging;

namespace FreieWahl.Application.Voting
{
    public class VotingManager : IVotingManager
    {
        private readonly IVotingStore _votingStore;
        private readonly ILogger _logger;

        public VotingManager(IVotingStore votingStore)
        {
            _logger = LogFactory.CreateLogger("Application.Registrations.BudgetSmsChallengeService");
            _votingStore = votingStore;
        }

        public Task Insert(StandardVoting voting)
        {
            return _votingStore.Insert(voting);
        }

        public async Task Update(StandardVoting voting)
        {
            await _CheckVotingEditable(voting.Id);
            await _votingStore.Update(voting);
        }

        public async Task<StandardVoting> GetById(string id)
        {
            var voting = await _votingStore.GetById(id);
            if (voting == null)
            {
                _logger.LogWarning("Could not find voting with id " + id);
                return null;
            }
            await _UpdateVotingStates(voting);

            return voting;
        }

        private async Task _UpdateVotingStates(StandardVoting voting)
        {
            if (DateTime.UtcNow > voting.EndDate && voting.State != VotingState.Closed)
            {
                _logger.LogInformation("Voting is over for voting with id " + voting.Id);
                if (voting.Questions.Any(x => x.Status != QuestionStatus.Locked))
                {
                    foreach (var question in voting.Questions)
                    {
                        question.Status = QuestionStatus.Locked;
                    }

                    voting.State = VotingState.Closed;
                    await _votingStore.Update(voting).ConfigureAwait(false);
                }
                else
                {
                    await _votingStore.UpdateState(voting.Id, VotingState.Closed).ConfigureAwait(false);
                    voting.State = VotingState.Closed;
                }
            }
        }

        public async Task<IEnumerable<StandardVoting>> GetForUserId(string userId)
        {
            var votings = await _votingStore.GetForUserId(userId).ConfigureAwait(false);
            var votingList = votings.ToList();
            foreach (var voting in votingList)
            {
                await _UpdateVotingStates(voting).ConfigureAwait(false);
            }

            return votingList;
        }

        private async Task _CheckVotingEditable(string votingId)
        {
            var voting = await _votingStore.GetById(votingId).ConfigureAwait(false);
            if (voting.State == VotingState.Closed)
            {
                _logger.LogError("Someone tried to modify a voting that has already been closed " + votingId);
                throw new InvalidOperationException("This voting has already been closed and cannot be edited anymore");
            }
        }

        public async Task AddQuestion(string votingId, Question question)
        {
            await _CheckVotingEditable(votingId).ConfigureAwait(false);
            await _votingStore.AddQuestion(votingId, question).ConfigureAwait(false);
        }

        public async Task DeleteQuestion(string votingId, int questionIndex)
        {
            await _CheckVotingEditable(votingId).ConfigureAwait(false);
            await _votingStore.DeleteQuestion(votingId, questionIndex).ConfigureAwait(false);
        }

        public async Task UpdateQuestion(string votingId, Question question)
        {
            await _CheckVotingEditable(votingId).ConfigureAwait(false);
            await _votingStore.UpdateQuestion(votingId, question).ConfigureAwait(false);
        }

        public async Task UpdateState(string votingId, VotingState state)
        {
            await _CheckVotingEditable(votingId).ConfigureAwait(false);
            await _votingStore.UpdateState(votingId, state).ConfigureAwait(false);
        }

        public async Task Delete(string votingId)
        {
            _logger.LogInformation("Deleting voting with id " + votingId);
            await _CheckVotingEditable(votingId).ConfigureAwait(false);
            await _votingStore.Delete(votingId).ConfigureAwait(false);
        }
    }
}
