using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreieWahl.Mail
{
    public interface IMailProvider
    {
        Task<SendResult> SendMail(List<string> recipientAddresses, string subject, string content,
            Dictionary<string, string> args);
    }
}
