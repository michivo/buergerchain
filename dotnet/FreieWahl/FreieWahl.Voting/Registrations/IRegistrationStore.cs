using System.Threading.Tasks;

namespace FreieWahl.Voting.Registrations
{
    public interface IRegistrationStore
    {
        Task AddRegistration(Registration registration);
        Task<Registration> GetRegistration(long id);
    }
}