using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreieWahl.Mail
{
    public interface IMailProvider
    {
        Task<SendResult> SendMail(string recipientName, string recipientAddress, string subject, string content,
            Dictionary<string, string> args);
    }
}
