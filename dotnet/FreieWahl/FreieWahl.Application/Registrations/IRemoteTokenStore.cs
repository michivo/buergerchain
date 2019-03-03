using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using Org.BouncyCastle.Crypto.Parameters;

namespace FreieWahl.Application.Registrations
{
    /// <summary>
    /// interface for the communication with the tokenstore application, which is an integral part of
    /// project buergerchain, but hosted in a separate app engine instance
    /// </summary>
    public interface IRemoteTokenStore
    {
        /// <summary>
        /// grants a given registration
        /// </summary>
        /// <param name="registrationStoreId">the id of the registration</param>
        /// <param name="voting">the voting the registration belongs to</param>
        /// <param name="signedChallengeString">a signed challenge proving that this request actually originates from this application</param>
        /// <param name="signedTokens">the list of signed, blinded voting tokens</param>
        /// <param name="votingUrl">the url for the voting</param>
        /// <param name="utcOffset">the utc offset of the voting administrator who granted the registration</param>
        /// <param name="timezoneName">the name of the timezone of the voting administrator who granted the registration</param>
        /// <returns>the response from the token store</returns>
        Task<string> GrantRegistration(string registrationStoreId, 
            StandardVoting voting,
            string signedChallengeString, 
            List<string> signedTokens, 
            string votingUrl,
            TimeSpan utcOffset,
            string timezoneName);

        /// <summary>
        /// gets a challenge for a given voting registration. This challenge will be signed when granting a registration, thus proving the authority to grant a registration.
        /// </summary>
        /// <param name="registrationId">the registration</param>
        /// <returns>the challenge that needs to be signed later on</returns>
        Task<RegistrationChallenge> GetChallenge(string registrationId);

        /// <summary>
        /// sends the public keys for a given voting to the token store. this can be used by the token store to verify the signature of the signed blinded keys
        /// </summary>
        /// <param name="votingId">the voting id</param>
        /// <param name="publicKeys">the list of public keys for all questions in the voting</param>
        /// <returns>the future of this operation</returns>
        Task InsertPublicKeys(string votingId, IEnumerable<RsaKeyParameters> publicKeys);
    }
}