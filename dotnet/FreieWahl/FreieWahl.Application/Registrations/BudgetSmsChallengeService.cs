﻿using System;
using System.Net;
using System.Threading.Tasks;
using FreieWahl.Application.Tracking;
using FreieWahl.Common;
using FreieWahl.Voting.Registrations;
using Microsoft.Extensions.Logging;

namespace FreieWahl.Application.Registrations
{
    public class BudgetSmsChallengeService : IChallengeService
    {
        private readonly ITracker _tracker;
        private readonly string _userName;
        private readonly string _userId;
        private readonly string _handle;
        private readonly string _apiUrl;
        private readonly ILogger _logger;

        public BudgetSmsChallengeService(ITracker tracker, string userName, string userId, string handle,
            bool isTestService = false)
        {
            _logger = LogFactory.CreateLogger("Application.Registrations.BudgetSmsChallengeService");
            _tracker = tracker;
            _userName = userName;
            _userId = userId;
            _handle = handle;
            _apiUrl = isTestService ? "https://api.budgetsms.net/testsms/" : "https://api.budgetsms.net/sendsms/";
        }

        public string GetStandardizedRecipient(string recipient)
        {
            return _FixPhoneNumber(recipient);
        }

        public async Task SendChallenge(string recipient, string challenge, string votingName, string votingId)
        {
            _logger.LogInformation($"Sending challenge for voting {votingName} to {recipient}");
            recipient = _FixPhoneNumber(recipient);
            WebClient client = new WebClient();
            client.QueryString.Add("username", _userName);
            client.QueryString.Add("userid", _userId);
            client.QueryString.Add("handle", _handle);
            if (votingName.Length > 60)
            {
                votingName = votingName.Substring(0, 60);
            }
            client.QueryString.Add("msg", $"Ihr Registrierungscode für Abstimmung '{votingName}' auf freiewahl.eu lautet {challenge}");
            client.QueryString.Add("from", "freiewahl");
            client.QueryString.Add("to", _FixPhoneNumber(recipient));
            client.QueryString.Add("price", "1");
            client.QueryString.Add("credit", "1");
            var result = await client.DownloadStringTaskAsync(_apiUrl);
            _logger.LogInformation($"Result for sending SMS: {result}");

            if (!result.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Fehler beim Senden der SMS: " + result);

            _TrackSmsCosts(result, votingId);
        }

        private void _TrackSmsCosts(string result, string votingId)
        {
            try
            {
                var resParts = result.Split(' ');
                if (resParts.Length < 3)
                    return;

                string cost = resParts[2];
                _tracker.TrackSpending(votingId, cost);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error tracking SMS cost: {ex}");
            }
        }

        public ChallengeType SupportedChallengeType => ChallengeType.Sms;

        private string _FixPhoneNumber(string number)
        {
            number = number.Replace(" ", "");
            number = number.Replace("-", "");
            number = number.Replace("/", "");
            if (number.StartsWith("+"))
            {
                return number.Substring(1);
            }

            if (number.StartsWith("00"))
            {
                return number.Substring(2);
            }

            if (number.StartsWith("0"))
            {
                return "43" + number.Substring(1); // defaults to austria
            }

            return number;
        }
    }
}

