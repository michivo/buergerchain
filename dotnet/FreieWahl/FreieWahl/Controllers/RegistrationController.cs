using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using FreieWahl.Application.Authentication;
using FreieWahl.Application.Registrations;
using FreieWahl.Common;
using FreieWahl.Security.Signing.Buergerkarte;
using FreieWahl.Voting.Registrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FreieWahl.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ISignatureHandler _signatureHandler;
        private readonly IRegistrationStore _registrationStore;
        private readonly IAuthorizationHandler _authHandler;
        private readonly IRegistrationHandler _registrationHandler;
        private static char _tokenFieldSeparator = '_';
        private string _regUrl;

        public RegistrationController(ILogger<RegistrationController> logger,
            ISignatureHandler signatureHandler,
            IRegistrationStore registrationStore,
            IAuthorizationHandler authHandler,
            IRegistrationHandler registrationHandler,
            IConfiguration configuration)
        {
            _signatureHandler = signatureHandler;
            _registrationStore = registrationStore;
            _authHandler = authHandler;
            _registrationHandler = registrationHandler;
            _regUrl = configuration["RemoteTokenStore:Url"];
        }

        [HttpPost]
        public async Task<IActionResult> Register(string regUid)
        {
            var response = Request.Form["XMLResponse"];

            var doc = new XmlDocument();
            doc.LoadXml(response);
            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
            manager.AddNamespace("sl", "http://www.buergerkarte.at/namespaces/securitylayer/1.2#");
            var signedData = doc.SelectSingleNode("//sl:CreateCMSSignatureResponse/sl:CMSSignature", manager).InnerText;
            var data = _signatureHandler.GetSignedContent(signedData);

            var dataContent = data.Data;
            var separatorIdx = dataContent.IndexOf(_tokenFieldSeparator);
            if (separatorIdx < 1 || separatorIdx >= dataContent.Length - 1)
                return BadRequest();

            var votingIdPart = dataContent.Substring(0, separatorIdx);
            var votingId = votingIdPart.ToId();
            if (votingId == null)
                return BadRequest();

            var mail = dataContent.Substring(separatorIdx + 1);

            await _registrationStore.AddOpenRegistration(new OpenRegistration
            {
                VotingId = votingId.Value,
                VoterIdentity = data.SigneeId,
                VoterName = data.SigneeName,
                RegistrationTime = DateTime.UtcNow,
                RegistrationStoreId = regUid,
                EMailAdress = mail
            });

            return Ok(votingIdPart);
        }

        public IActionResult RegistrationDetails(string regUid)
        {
            ViewData["RegistrationStoreId"] = regUid;
            ViewData["RegistrationStoreSaveRegUrl"] = _regUrl + "saveRegistrationDetails";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GrantRegistration(string rid)
        {
            var regId = rid.ToId();
            if (regId == null)
            {
                return BadRequest();
            }

            var registration = await _registrationStore.GetOpenRegistration(regId.Value);
            string vid = registration.VotingId.ToString(CultureInfo.InvariantCulture);
            var user = await _authHandler.GetAuthorizedUser(vid,
                Operation.GrantRegistration, Request.Headers["Authorization"]);
            if (user == null)
            {
                return Unauthorized();
            }


            await _registrationHandler.GrantRegistration(regId.Value, user.UserId);
            return Ok();
        }

        public async Task<IActionResult> GetRegistrations(string votingId)
        {
            if (await _authHandler.CheckAuthorization(votingId,
                    Operation.GrantRegistration, Request.Headers["Authorization"]) == false)
            {
                return Unauthorized();
            }

            var votingIdVal = votingId.ToId();
            if (votingIdVal == null)
            {
                return BadRequest("Invalid voting id");
            }

            var registrations = await _registrationStore.GetOpenRegistrationsForVoting(votingIdVal.Value);
            var result = registrations.Select(x =>
                new
                {
                    x.VoterName,
                    x.VoterIdentity,
                    RegistrationId = x.RegistrationId.ToString(CultureInfo.InvariantCulture)
                }
            ).ToArray();

            return new JsonResult(result);
        }


    }
}
