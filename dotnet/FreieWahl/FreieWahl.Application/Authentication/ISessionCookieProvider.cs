using System.Threading.Tasks;

namespace FreieWahl.Application.Authentication
{
    /// <summary>
    /// Provides session cookies in order to allow a user staying logged in for a longer period
    /// </summary>
    public interface ISessionCookieProvider
    {
        /// <summary>
        /// creates a session cookie for a given session id token
        /// </summary>
        /// <param name="idToken">a session id token</param>
        /// <returns>a session cooke for the given id token</returns>
        Task<SessionCookie> CreateSessionCookie(string idToken);
    }
}
