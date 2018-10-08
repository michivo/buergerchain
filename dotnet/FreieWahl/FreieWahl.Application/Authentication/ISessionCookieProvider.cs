using System.Threading.Tasks;

namespace FreieWahl.Application.Authentication
{
    public interface ISessionCookieProvider
    {
        Task<SessionCookie> CreateSessionCookie(string idToken);
    }
}
