using System.Threading.Tasks;

namespace FreieWahl.Application.Authentication
{
    public interface IAuthorizationHandler
    {
        Task<AuthenticationResult> CheckAuthorization(string votingId, Operation operation, string authToken);
    }
}
