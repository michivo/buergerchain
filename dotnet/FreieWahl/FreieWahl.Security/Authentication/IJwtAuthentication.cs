using System.Threading.Tasks;

namespace FreieWahl.Security.Authentication
{
    public interface IJwtAuthentication
    {
        Task Initialize(string domain, string audience);

        JwtAuthenticationResult CheckToken(string token);
    }
}
