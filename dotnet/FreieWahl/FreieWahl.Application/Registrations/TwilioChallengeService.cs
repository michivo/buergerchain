using System.Threading.Tasks;
using FreieWahl.Common;
using FreieWahl.Voting.Registrations;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace FreieWahl.Application.Registrations
{
    public class TwilioChallengeService : IChallengeService
    {
        private readonly ILogger _logger;

        public TwilioChallengeService(string userId, string userKey)
        {
            TwilioClient.Init(userId, userKey);
            _logger = LogFactory.CreateLogger("Application.Registrations.TwilioChallengeService");
        }

        public string GetStandardizedRecipient(string recipient)
        {
            return _FixPhoneNumber(recipient);
        }

        public async Task SendChallenge(string recipient, string challenge, string votingName, string votingId)
        {
            _logger.LogInformation($"Sending challenge for voting {votingName} to {recipient}");
            string msgBody = $"Ihr Registrierungscode für Abstimmung '{votingName}' auf freiewahl.eu lautet {challenge}";

            var message = await MessageResource.CreateAsync(
                body: msgBody,
                to: new PhoneNumber(_FixPhoneNumber(recipient)),
                from: new PhoneNumber("freiewahl")
            );

            _logger.LogInformation($"Result for sending SMS: {message.Sid}");
        }

        private string _FixPhoneNumber(string number)
        {
            number = number.Replace(" ", "");
            number = number.Replace("-", "");
            number = number.Replace("/", "");
            if (number.StartsWith("+"))
            {
                return number;
            }

            if (number.StartsWith("00"))
            {
                return "+" + number.Substring(2);
            }

            if (number.StartsWith("0"))
            {
                return "+43" + number.Substring(1); // defaults to austria
            }

            return number;
        }

        public ChallengeType SupportedChallengeType => ChallengeType.Sms;
    }
}
