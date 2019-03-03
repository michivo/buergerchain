using System;
using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    /// <summary>
    /// interface for granting or denying a registration
    /// </summary>
    public interface IRegistrationHandler
    {
        /// <summary>
        /// grants a given registration. This also calls GrantRegistration in IRemoteTokenStore
        /// </summary>
        /// <param name="registrationId">the id of the registration</param>
        /// <param name="userId">the user granting the registration</param>
        /// <param name="votingUrl">the url for the voting</param>
        /// <param name="utcOffset">the utc offset of the voting administrator who granted the registration</param>
        /// <param name="timezoneName">the name of the timezone of the voting administrator who granted the registration</param>
        /// <returns>the future of this operation</returns>
        Task GrantRegistration(string registrationId, string userId, string votingUrl, TimeSpan utcOffset, string timezoneName);

        /// <summary>
        /// denies a given registration.
        /// </summary>
        /// <param name="registrationId">the id of the registration</param>
        /// <param name="userId">the user granting the registration</param>
        /// <returns>the future of this operation</returns>
        Task DenyRegistration(string registrationId, string userId);
    }
}