using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreieWahl.Voting.Registrations
{
    /// <summary>
    /// stores all open and completed registrations.
    /// </summary>
    public interface IRegistrationStore
    {
        /// <summary>
        /// adds an open registration
        /// </summary>
        /// <param name="openRegistration">the open registration</param>
        /// <returns>the future of this registration</returns>
        Task AddOpenRegistration(OpenRegistration openRegistration);

        /// <summary>
        /// adds a completed registration
        /// </summary>
        /// <param name="completedRegistration">the completed registration (granted or denied)</param>
        /// <returns>the future of this operation</returns>
        Task AddCompletedRegistration(CompletedRegistration completedRegistration);

        /// <summary>
        /// removes an open registration
        /// </summary>
        /// <param name="id">the registration id</param>
        /// <returns>the future of this operation</returns>
        Task RemoveOpenRegistration(string id);

        /// <summary>
        /// gets all open registrations for a given voting
        /// </summary>
        /// <param name="votingId">the voting id</param>
        /// <returns>a list of open registrations</returns>
        Task<IReadOnlyList<OpenRegistration>> GetOpenRegistrationsForVoting(string votingId);

        /// <summary>
        /// gets a single open registration
        /// </summary>
        /// <param name="registrationStoreId">the registration id</param>
        /// <returns>the registration with the given id</returns>
        Task<OpenRegistration> GetOpenRegistration(string registrationStoreId);

        /// <summary>
        /// gets all completed registrations for a given voting id
        /// </summary>
        /// <param name="votingId">the voting id</param>
        /// <returns>a list of completed registrations (denied or granted)</returns>
        Task<IReadOnlyList<CompletedRegistration>> GetCompletedRegistrations(string votingId);

        /// <summary>
        /// checks if someone with the given signee id has already registered for a given voting id - required to avoid double registrations
        /// </summary>
        /// <param name="dataSigneeId">the signee id of the voter (e.g. mobile phone number or buergerkarten-number)</param>
        /// <param name="votingId">the voting id</param>
        /// <returns>true, if there is no registration for the given signee id and voting id</returns>
        Task<bool> IsRegistrationUnique(string dataSigneeId, string votingId);
    }
}