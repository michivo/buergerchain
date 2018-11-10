using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FreieWahl.Mail.SendGrid
{
    public class SendGridMailProvider : IMailProvider
    {
        private readonly EmailAddress _fromAddress;
        private readonly SendGridClient _client;

        public SendGridMailProvider(string apiKey, string fromAddress, string fromName = null)
        {
            _fromAddress = new EmailAddress(fromAddress, fromName);
            _client = new SendGridClient(apiKey);
        }

        public async Task<SendResult> SendMail(List<string> recipientAddresses, string subject, string content,
            Dictionary<string, string> args)
        {
            foreach (var recipientAddress in recipientAddresses)
            {
                var message = new SendGridMessage();
                message.AddTo(recipientAddress);
                message.HtmlContent = content;
                message.Subject = subject;
                message.From = _fromAddress;
                message.AddSubstitutions(args);
                try
                {
                    await _client.SendEmailAsync(message).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return new SendResult(false, ex.Message);
                }
            }

            return SendResult.SuccessResult;
        }
    }
}
