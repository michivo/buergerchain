using System.Threading.Tasks;

namespace FreieWahl.Application.Authentication
{
    public interface IAuthenticationManager
    {
        Task<bool> IsAuthorized(string userId, string votingId, Operation operation);
    }
}
