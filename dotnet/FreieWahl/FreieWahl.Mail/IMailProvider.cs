using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreieWahl.Mail
{
    /// <summary>
    /// Implementations of this interface support sending e-mails to one or more recipients
    /// </summary>
    public interface IMailProvider
    {
        /// <summary>
        /// Sends a mail to one or more recipients. It also supports replacing placeholders in the content with values.
        /// </summary>
        /// <param name="recipientAddresses">the address(es) of the recipient(s)</param>
        /// <param name="subject">the mail subject</param>
        /// <param name="content">the mail content</param>
        /// <param name="args">a list of placeholders and values. All occurences of the placeholder(s) in the content are replaces with the corresponding value(s).</param>
        /// <returns>the result of the send process (true or false)</returns>
        Task<SendResult> SendMail(List<string> recipientAddresses, string subject, string content,
            Dictionary<string, string> args);
    }
}
