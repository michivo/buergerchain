using System.Threading.Tasks;

namespace FreieWahl.Security.Authentication
{
    /// <summary>
    /// Interface for checking JWT authorization tokens, this is essential for the authentication process.
    /// </summary>
    public interface IJwtAuthentication
    {
        /// <summary>
        /// Initializes an instance of the token checker
        /// </summary>
        /// <param name="certUrl">url of the public keys used to sign the JWT tokens (e.g. https://www.googleapis.com/identitytoolkit/v3/relyingparty/publicKeys) - only tokens signed with one of the corresponding private keys are accepted</param>
        /// <param name="issuer">issuer of the jwt token (e.g. https://session.firebase.google.com/freiewahl-application ) - only tokens with this issuer are accepted</param>
        /// <param name="audience">valid audience for jwt tokens (e.g. freiewahl-application) - only tokens published for this audience will be accepted</param>
        /// <returns>the future of this operation</returns>
        Task Initialize(string certUrl, string issuer, string audience);

        /// <summary>
        /// checks if a given jwt token is valid (see prerequisites above)
        /// </summary>
        /// <param name="token">a token</param>
        /// <returns>the validation result</returns>
        JwtAuthenticationResult CheckToken(string token);
    }
}
