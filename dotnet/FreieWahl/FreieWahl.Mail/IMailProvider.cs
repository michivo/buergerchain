using System;
using System.Threading.Tasks;

namespace FreieWahl.Mail
{
    public interface IMailProvider
    {
        Task<SendResult> SendMail(string recipientName, string recipientAddress, string subject, string content);
    }
}
