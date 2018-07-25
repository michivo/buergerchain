using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FreieWahl.Security.Signing.Common;
using FreieWahl.Voting.Registrations;

namespace FreieWahl.Application.Registrations
{
    public class RegistrationHandler : IRegistrationHandler
    {
        private readonly IRegistrationStore _store;
        private readonly ISignatureProvider _signatureProvider;
        private readonly IRemoteTokenStore _remoteTokenStore;

        public RegistrationHandler(IRegistrationStore store,
            ISignatureProvider signatureProvider,
            IRemoteTokenStore remoteTokenStore)
        {
            _store = store;
            _signatureProvider = signatureProvider;
            _remoteTokenStore = remoteTokenStore;
        }

        public async Task GrantRegistration(long registrationId, string challenge)
        {
            var signedChallenge = _signatureProvider.SignData(Encoding.UTF8.GetBytes(challenge));
            var signedChallengeString = Convert.ToBase64String(signedChallenge);
            await _remoteTokenStore.GrantRegistration(registrationId, signedChallengeString);
            await _store.RemoveRegistration(registrationId);
        }

        public Task DenyRegistration(long registrationId)
        {
            return _store.RemoveRegistration(registrationId);
        }
    }
}
