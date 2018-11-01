using System;
using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    public interface IRegistrationHandler
    {
        Task GrantRegistration(string registrationId, string userId, string votingUrl, TimeSpan utcOffset, string timezoneName);

        Task DenyRegistration(string registrationId, string userId);
    }
}