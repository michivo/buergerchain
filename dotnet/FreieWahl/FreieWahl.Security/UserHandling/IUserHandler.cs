using System.Security.Claims;

namespace FreieWahl.Security.UserHandling
{
    public interface IUserHandler
    {
        UserInformation MapUser(ClaimsPrincipal result);
    }
}
