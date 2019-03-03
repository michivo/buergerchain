using System.Threading.Tasks;
using FreieWahl.Security.UserHandling;

namespace FreieWahl.Application.Authentication
{
    /// <summary>
    /// Checks if a user is authorized to perform a certain action. There is only a pretty simple implementation,
    /// since there is a pretty simple user system implemented at the moment.
    /// </summary>
    public interface IAuthenticationManager
    {
        /// <summary>
        /// Checks if a user is authorized to perform a certain action
        /// </summary>
        /// <param name="userId">the user's id</param>
        /// <param name="votingId">the id of voting that is affected by this operation</param>
        /// <param name="operation">the operation that is about to be performed</param>
        /// <returns>result of the authorization (granted or not)</returns>
        Task<AuthenticationResult> IsAuthorized(UserInformation userId, string votingId, Operation operation);
    }
}
