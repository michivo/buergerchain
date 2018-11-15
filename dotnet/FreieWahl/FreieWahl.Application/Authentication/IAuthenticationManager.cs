using System.Threading.Tasks;
using FreieWahl.Security.UserHandling;

namespace FreieWahl.Application.Authentication
{
    public interface IAuthenticationManager
    {
        Task<AuthenticationResult> IsAuthorized(UserInformation userId, string votingId, Operation operation);
    }
}
