using System;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FreieWahl.Mail.SendGrid
{
    public class SendGridMailProvider : IMailProvider
    {
        private readonly string _fromAddress;
        private readonly string _fromName;
        private readonly SendGridClient _client;

        public SendGridMailProvider(string apiKey, string fromAddress, string fromName = null)
        {
            _fromAddress = fromAddress;
            _fromName = fromName;
            _client = new SendGridClient(apiKey);
        }

        public async Task<SendResult> SendMail(string recipientName, string recipientAddress, string subject, string content)
        {
            var message = new SendGridMessage();
            message.AddTo(recipientAddress, recipientName);
            message.PlainTextContent = content;
            message.Subject = subject;
            message.From = new EmailAddress(_fromAddress, _fromName);
            try
            {
                await _client.SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                return new SendResult(false, ex.Message);
            }

            return SendResult.SuccessResult;
        }
    }
}
