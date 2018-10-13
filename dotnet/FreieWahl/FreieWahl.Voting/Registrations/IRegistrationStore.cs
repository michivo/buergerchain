using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreieWahl.Voting.Registrations
{
    public interface IRegistrationStore
    {
        Task AddOpenRegistration(OpenRegistration openRegistration);

        Task AddCompletedRegistration(CompletedRegistration openRegistration);

        Task<OpenRegistration> GetOpenRegistration(long id);

        Task RemoveOpenRegistration(long id);

        Task<IReadOnlyList<OpenRegistration>> GetOpenRegistrationsForVoting(long votingId);

        Task<OpenRegistration> GetOpenRegistration(string registrationStoreId);

        Task<IReadOnlyList<CompletedRegistration>> GetCompletedRegistrations(long votingId);

        Task<bool> IsRegistrationUnique(string dataSigneeId, long votingId);
    }
}