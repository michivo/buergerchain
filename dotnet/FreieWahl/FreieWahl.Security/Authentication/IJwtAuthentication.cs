using System.Threading.Tasks;

namespace FreieWahl.Security.Authentication
{
    public interface IJwtAuthentication
    {
        Task Initialize(string certUrl, string issuer, string audience);

        JwtAuthenticationResult CheckToken(string token);
    }
}
