using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FreieWahl.Security.Signing.Common;
using FreieWahl.Security.Signing.VotingTokens;
using FreieWahl.Voting.Registrations;
using FreieWahl.Voting.Storage;

namespace FreieWahl.Application.Registrations
{
    public class RegistrationHandler : IRegistrationHandler
    {
        private readonly IRegistrationStore _store;
        private readonly ISignatureProvider _signatureProvider;
        private readonly IRemoteTokenStore _remoteTokenStore;
        private readonly IVotingTokenHandler _votingTokenHandler;
        private readonly IVotingStore _votingStore;

        public RegistrationHandler(IRegistrationStore store,
            ISignatureProvider signatureProvider,
            IRemoteTokenStore remoteTokenStore,
            IVotingTokenHandler votingTokenHandler,
            IVotingStore votingStore)
        {
            _store = store;
            _signatureProvider = signatureProvider;
            _remoteTokenStore = remoteTokenStore;
            _votingTokenHandler = votingTokenHandler;
            _votingStore = votingStore;
        }

        public async Task GrantRegistration(long registrationId, string userId, string votingUrl)
        {
            var registration = await _store.GetOpenRegistration(registrationId);
            var challenge = await _remoteTokenStore.GetChallenge(registration.RegistrationStoreId);
            var signedChallenge = _signatureProvider.SignData(Encoding.UTF8.GetBytes(challenge.Challenge));
            var signedChallengeString = Convert.ToBase64String(signedChallenge);
            var signedTokens = _SignTokens(registration, challenge.Tokens);
            var voting = await _votingStore.GetById(registration.VotingId);

            await _remoteTokenStore.GrantRegistration(registration.RegistrationStoreId, 
                voting, signedChallengeString, signedTokens, votingUrl);
            var completedReg = new CompletedRegistration()
            {
                VotingId = registration.VotingId,
                DecisionTime = DateTime.UtcNow,
                Decision = RegistrationDecision.Granted,
                RegistrationTime = registration.RegistrationTime,
                VoterIdentity = registration.VoterIdentity,
                VoterName = registration.VoterName,
                AdminUserId = userId
            };

            await _store.RemoveOpenRegistration(registrationId);
            await _store.AddCompletedRegistration(completedReg);
        }

        private List<string> _SignTokens(OpenRegistration registration, List<string> challengeTokens)
        {
            var result = new List<string>();
            for (int i = 0; i < challengeTokens.Count; i++)
            {
                var signedToken = _votingTokenHandler.Sign(challengeTokens[i], registration.VotingId, i);
                result.Add(signedToken);
            }

            return result;
        }

        public async Task DenyRegistration(long registrationId, string userId)
        {
            var registration = await _store.GetOpenRegistration(registrationId);
            var completedReg = new CompletedRegistration()
            {
                VotingId = registration.VotingId,
                DecisionTime = DateTime.UtcNow,
                Decision = RegistrationDecision.Denied,
                RegistrationTime = registration.RegistrationTime,
                VoterIdentity = registration.VoterIdentity,
                VoterName = registration.VoterName,
                AdminUserId = userId
            };
            await _store.RemoveOpenRegistration(registrationId);
            await _store.AddCompletedRegistration(completedReg);
        }
    }
}
