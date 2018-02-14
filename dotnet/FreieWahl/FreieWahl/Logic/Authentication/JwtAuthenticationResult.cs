using System;
using System.Security.Claims;

namespace FreieWahl.Logic.Authentication
{
    public sealed class JwtAuthenticationResult
    {
        public JwtAuthenticationResult(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
            User = null;
        }

        public JwtAuthenticationResult(ClaimsPrincipal user)
        {
            User = user;
            Success = true;
            ErrorMessage = String.Empty;
        }

        public ClaimsPrincipal User { get; }

        public bool Success { get; }

        public String ErrorMessage { get; }
    }
}
