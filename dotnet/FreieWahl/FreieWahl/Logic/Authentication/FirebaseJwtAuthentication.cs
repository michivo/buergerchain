using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace FreieWahl.Logic.Authentication
{
    public class FirebaseJwtAuthentication : IJwtAuthentication
    {
        private TokenValidationParameters _validationParameters;
        private bool _isInitialized;

        public async Task Initialize(string domain, string audience)
        {
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager = 
                new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{domain}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration openIdConfig = 
                await configurationManager.GetConfigurationAsync(CancellationToken.None);

            // Configure the TokenValidationParameters. Assign the SigningKeys which were downloaded from Auth0. 
            // Also set the Issuer and Audience(s) to validate

            _validationParameters = new TokenValidationParameters
            {
                ValidIssuer = domain,
                ValidAudience = audience,
                IssuerSigningKeys = openIdConfig.SigningKeys,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true
            };

            _isInitialized = true;
        }

        public JwtAuthenticationResult CheckToken(string token)
        {
            if(!_isInitialized)
                return new JwtAuthenticationResult("Not initalized yet"); // TODO: exception?

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var user = handler.ValidateToken(token, _validationParameters, out _);

            if(user != null)
                return new JwtAuthenticationResult(user);

            return new JwtAuthenticationResult("Verification failed");
        }
    }
}

