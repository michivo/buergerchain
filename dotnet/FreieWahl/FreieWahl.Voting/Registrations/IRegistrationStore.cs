using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreieWahl.Voting.Registrations
{
    public interface IRegistrationStore
    {
        Task AddRegistration(Registration registration);

        Task<Registration> GetRegistration(long id);

        Task RemoveRegistration(long id);

        Task<IReadOnlyList<Registration>> GetRegistrationsForVoting(long votingId);

        Task<Registration> GetRegistration(string registrationStoreId);
    }
}