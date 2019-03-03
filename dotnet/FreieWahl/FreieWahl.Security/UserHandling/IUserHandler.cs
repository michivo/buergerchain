using System.Security.Claims;

namespace FreieWahl.Security.UserHandling
{
    /// <summary>
    /// Maps user information from a JWT auth token to user information including user name, id and mail address
    /// </summary>
    public interface IUserHandler
    {
        /// <summary>
        /// Maps user information from a JWT auth token to user information including user name, id and mail address
        /// </summary>
        /// <param name="result">user claims from a JWT auth token</param>
        /// <returns>the extracted user information (user name, id and mail address)</returns>
        UserInformation MapUser(ClaimsPrincipal result);
    }
}
