using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FreieWahl.Application.Authentication;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.Signing.Buergerkarte;
using FreieWahl.Security.Signing.Common;
using FreieWahl.Security.Signing.VotingTokens;
using FreieWahl.Security.UserHandling;
using FreieWahl.Voting.Registrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FreieWahl.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ISignatureHandler _signatureHandler;
        private readonly IRegistrationStore _registrationStore;
        private readonly ISignatureProvider _signatureProvider;
        private readonly IVotingTokenHandler _votingTokenHandler;
        private readonly IAuthorizationHandler _authHandler;
        private UserInformation _user;

        public RegistrationController(ILogger<RegistrationController> logger,
            ISignatureHandler signatureHandler,
            IRegistrationStore registrationStore,
            ISignatureProvider signatureProvider,
            IVotingTokenHandler votingTokenHandler,
            IAuthorizationHandler authHandler)
        {
            _signatureHandler = signatureHandler;
            _registrationStore = registrationStore;
            _signatureProvider = signatureProvider;
            _votingTokenHandler = votingTokenHandler;
            _authHandler = authHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Register()
        {
            var response = Request.Form["XMLResponse"];

            var doc = new XmlDocument();
            doc.LoadXml(response);
            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
            manager.AddNamespace("sl", "http://www.buergerkarte.at/namespaces/securitylayer/1.2#");
            var signedData = doc.SelectSingleNode("//sl:CreateCMSSignatureResponse/sl:CMSSignature", manager).InnerText;
            var data = _signatureHandler.GetSignedContent(signedData);

            var jObject = JObject.Parse(data.Data);
            var votingId = (string)jObject.GetValue("VotingId", StringComparison.OrdinalIgnoreCase);
            var signeeId = data.SigneeId;
            var signeeName = data.SigneeName;
            var tokens = (JArray)jObject.GetValue("Tokens", StringComparison.OrdinalIgnoreCase);
            var signedTokens = new List<string>();
            var votingIdVal = long.Parse(votingId);
            foreach (var token in tokens)
            {
                var index = (int)token["Index"];
                var tokenValue = (string)token["Token"];
                signedTokens.Add(await _votingTokenHandler.Sign(tokenValue, votingIdVal, index));
            }

            var registration = new Registration
            {
                VoterName = signeeName,
                VotingId = long.Parse(votingId),
                VoterIdentity = signeeId
            };
            await _registrationStore.AddRegistration(registration);

            var regId = registration.RegistrationId.ToString(CultureInfo.InvariantCulture);
            var regIdSigned = _signatureProvider.SignData(BitConverter.GetBytes(registration.RegistrationId));
            var regIdSignedString = Convert.ToBase64String(regIdSigned);
            return new JsonResult(new
            {
                RegistrationId = regId,
                RegistrationIdSigned = regIdSignedString,
                SignedTokens = signedTokens
            });
        }

        [HttpPost]
        public async Task<IActionResult> UnlockRegistration(string votingId, string registrationId)
        {
            if (await _authHandler.CheckAuthorization(votingId,
                    Operation.GrantRegistration, Request.Headers["Authorization"]) == false)
            {
                return Unauthorized();
            }

            return Ok();
        }
    }
}
