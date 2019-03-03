using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace FreieWahl.Security.Authentication
{
    public class FirebaseJwtAuthentication : IJwtAuthentication
    {
        private TokenValidationParameters _validationParameters;
        private bool _isInitialized;

        public async Task Initialize(string certUrl, string issuer, string audience)
        {
            var result = await _GetCertificateData(certUrl);

            var jResult = JObject.Parse(result);
            var signingKeys = new List<SecurityKey>();
            foreach (var t in jResult)
            {
                signingKeys.Add(_GetCertificate(t.Value.ToString()));
            }

            _validationParameters = new TokenValidationParameters
            {
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKeys = signingKeys,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };

            _isInitialized = true;
        }

        private SecurityKey _GetCertificate(string certificateData)
        {
            var startIdx = certificateData.IndexOf('\n');
            var endIdx = certificateData.IndexOf("\n-----END CERTIFICATE-----", StringComparison.OrdinalIgnoreCase);
            var rawCertificate = certificateData.Substring(startIdx, endIdx - startIdx);
            return new X509SecurityKey(new X509Certificate2(Convert.FromBase64String(rawCertificate)));
        }

        private static async Task<string> _GetCertificateData(string certUrl)
        {
            var request =
                WebRequest.CreateHttp(certUrl); //"https://www.googleapis.com/identitytoolkit/v3/relyingparty/publicKeys"
            var resp = await request.GetResponseAsync();
            string result;
            using (var streamReader = new StreamReader(resp.GetResponseStream()))
            {
                // TODO: error handling
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public JwtAuthenticationResult CheckToken(string token)
        {
            if(!_isInitialized)
                return new JwtAuthenticationResult("Not initalized yet"); // TODO: exception?

            if(string.IsNullOrEmpty(token))
                return new JwtAuthenticationResult("Missing authentication header");

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            try
            {
                var user = handler.ValidateToken(token, _validationParameters, out _);
                if (user != null)
                    return new JwtAuthenticationResult(user);
            }
            catch (Exception)
            {
                // TODO logging
                return new JwtAuthenticationResult("Verification failed with exception!");
            }

            return new JwtAuthenticationResult("Verification failed");
        }
    }
}

