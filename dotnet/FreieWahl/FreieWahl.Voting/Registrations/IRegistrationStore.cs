using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreieWahl.Voting.Registrations
{
    public interface IRegistrationStore
    {
        Task AddOpenRegistration(OpenRegistration openRegistration);

        Task AddCompletedRegistration(CompletedRegistration openRegistration);

        Task RemoveOpenRegistration(string id);

        Task<IReadOnlyList<OpenRegistration>> GetOpenRegistrationsForVoting(string votingId);

        Task<OpenRegistration> GetOpenRegistration(string registrationStoreId);

        Task<IReadOnlyList<CompletedRegistration>> GetCompletedRegistrations(string votingId);

        Task<bool> IsRegistrationUnique(string dataSigneeId, string votingId);
    }
}