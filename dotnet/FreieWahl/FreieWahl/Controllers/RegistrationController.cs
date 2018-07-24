using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using FreieWahl.Security.Signing.Buergerkarte;
using FreieWahl.Security.Signing.Common;
using FreieWahl.Security.Signing.VotingTokens;
using FreieWahl.Voting.Registrations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FreieWahl.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ISignatureHandler _signatureHandler;
        private readonly IRegistrationStore _registrationStore;
        private readonly ISignatureProvider _signatureProvider;

        public RegistrationController(
            ISignatureHandler signatureHandler,
            IRegistrationStore registrationStore,
            ISignatureProvider signatureProvider)
        {
            _signatureHandler = signatureHandler;
            _registrationStore = registrationStore;
            _signatureProvider = signatureProvider;
        }

        [HttpPost]
        public IActionResult Register()
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

            var registration = new Registration
            {
                VoterName = signeeName,
                VotingId = long.Parse(votingId),
                VoterIdentity = signeeId
            };
            _registrationStore.AddRegistration(registration);

            var regId = registration.RegistrationId.ToString(CultureInfo.InvariantCulture);
            var regIdSigned = _signatureProvider.SignData(BitConverter.GetBytes(registration.RegistrationId));
            var regIdSignedString = Convert.ToBase64String(regIdSigned);
            return new JsonResult(new
            {
                RegistrationId = regId,
                RegistrationIdSigned = regIdSignedString
            });
        }
    }
}
