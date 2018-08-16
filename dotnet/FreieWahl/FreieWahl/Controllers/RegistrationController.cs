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

            var votingId = data.Data;
            long votingIdVal = long.Parse(votingId); // TODO err handling
            await _registrationStore.AddRegistration(new Registration
            {
                VotingId = votingIdVal,
                VoterIdentity = data.SigneeId,
                VoterName = data.SigneeName
            });

            return Ok();
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
