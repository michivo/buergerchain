using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Registrations;
using Microsoft.Extensions.Configuration;

namespace FreieWahl.Application.Registrations
{
    public class DefaultChallengeHandler : IChallengeHandler
    {
        private readonly IChallengeStore _challengeStore;
        private readonly Random _random;
        private readonly IChallengeService[] _challengeServices;

        public DefaultChallengeHandler(IChallengeStore challengeStore,
            IConfiguration configuration)
        {
            _challengeStore = challengeStore;
            _random = new Random();
            _challengeServices = new IChallengeService[]
            {
                new BudgetSmsChallengeService(
                    configuration["BudgetSms:Username"],
                    configuration["BudgetSms:UserId"],
                    configuration["BudgetSms:Handle"], true)
            };
        }

        public async Task CreateChallenge(string recipientName, string recipientAddress, StandardVoting voting, string registrationId, ChallengeType challengeType)
        {
            var challengeService = _challengeServices.FirstOrDefault(x => x.SupportedChallengeType == challengeType);
            if (challengeService == null)
            {
                throw new ArgumentException("No challenge service is implemented for the given challenge type!");
            }

            var value = _random.Next(100000, 1000000).ToString(CultureInfo.InvariantCulture);

            var challenge = new Challenge
            {
                RegistrationId = registrationId,
                Type = challengeType,
                VotingId = voting.Id,
                Value = value,
                RecipientName = recipientName,
                RecipientAddress = challengeService.GetStandardizedRecipient(recipientAddress)
            };

            await Task.WhenAll(
                challengeService.SendChallenge(challenge.RecipientAddress, value, voting.Title),
                _challengeStore.SetChallenge(challenge)).ConfigureAwait(false);
        }

        public async Task<Challenge> GetChallengeForRegistration(string registrationId)
        {
            try
            {
                var challenge = await _challengeStore.GetChallenge(registrationId);
                await _challengeStore.DeleteChallenge(registrationId);
                return challenge;
            }
            catch (Exception)
            {
                // todo Logging
                return null;
            }
        }
    }
}