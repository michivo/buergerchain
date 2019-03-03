using System.Threading.Tasks;

namespace FreieWahl.Application.Authentication
{
    /// <summary>
    /// Checks if a user providing the given authorization token is allowed to perform a certain operation
    /// </summary>
    public interface IAuthorizationHandler
    {
        /// <summary>
        /// Checks if a user is authorized to perform a certain action
        /// </summary>
        /// <param name="authToken">the authToken provided by a user. This can be mapped to a user</param>
        /// <param name="votingId">the id of voting that is affected by this operation</param>
        /// <param name="operation">the operation that is about to be performed</param>
        /// <returns>result of the authorization (granted or not)</returns>
        Task<AuthenticationResult> CheckAuthorization(string votingId, Operation operation, string authToken);
    }
}
