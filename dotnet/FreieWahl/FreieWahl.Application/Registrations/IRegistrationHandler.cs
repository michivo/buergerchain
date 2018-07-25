using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    public interface IRegistrationHandler
    {
        Task GrantRegistration(long registrationId, string challenge);

        Task DenyRegistration(long registrationId);
    }
}