using System;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;

namespace FreieWahl.Security.UserHandling
{
    public class UserHandler : IUserHandler
    {
        private const string nameClaim = "name";
        private const string mailClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        private const string idClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        public UserInformation MapUser(ClaimsPrincipal principal)
        {
            var name = principal.Claims.FirstOrDefault(x => x.Type == nameClaim);
            var mail = principal.Claims.FirstOrDefault(x => x.Type == mailClaim);
            var id = principal.Claims.FirstOrDefault(x => x.Type == idClaim);

            if (name == null || id == null || mail == null)
                throw new ArgumentException("Invalid principal: Missing critical information");

            if (string.IsNullOrEmpty(name.Value) || 
                string.IsNullOrEmpty(id.Value) || 
                string.IsNullOrEmpty(mail.Value))
                throw new ArgumentException("Invalid principal: Critical information is empty");

            return new UserInformation(name.Value, id.Value, new MailAddress(mail.Value, name.Value));
        }
    }
}
