using System;

namespace FreieWahl.Application.Authentication
{
    public class SessionCookie
    {
        public SessionCookie(string token, DateTimeOffset maxAge)
        {
            Token = token;
            MaxAge = maxAge;
        }

        public string Token { get; }

        public DateTimeOffset MaxAge { get; }
    }
}
