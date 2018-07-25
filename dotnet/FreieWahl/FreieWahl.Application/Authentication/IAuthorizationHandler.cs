using System.Threading.Tasks;
using FreieWahl.Security.UserHandling;

namespace FreieWahl.Application.Authentication
{
    public interface IAuthorizationHandler
    {
        Task<bool> CheckAuthorization(string votingId, Operation operation, string authToken);

        Task<UserInformation> GetAuthorizedUser(string votingId, Operation operation, string authToken);
    }
}
