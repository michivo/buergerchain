using System.Net.Mail;

namespace FreieWahl.Security.UserHandling
{
    public class UserInformation
    {
        public UserInformation(string name, string userId, MailAddress mailAddress)
        {
            Name = name;
            UserId = userId;
            MailAddress = mailAddress;
        }

        public string Name { get; set; }

        public string UserId { get; set; }

        public MailAddress MailAddress { get; set; }
    }
}
