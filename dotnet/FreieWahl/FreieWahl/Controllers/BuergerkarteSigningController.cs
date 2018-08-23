using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using FreieWahl.Security.Signing.VotingTokens;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FreieWahl.Security.Signing.Buergerkarte
{
    public class BuergerkarteSigningController : Controller
    {
        private readonly ISignatureHandler _signatureHandler;
        private readonly IVotingTokenHandler _votingTokenHandler;

        public BuergerkarteSigningController(ISignatureHandler signatureHandler,
            IVotingTokenHandler votingTokenHandler)
        {
            _signatureHandler = signatureHandler;
            _votingTokenHandler = votingTokenHandler;
        }

        [HttpPost]
        public async Task<IActionResult> SignatureDataUrl()
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
            
            var tokens = (JArray)jObject.GetValue("Tokens", StringComparison.OrdinalIgnoreCase);
            var signedTokens = new List<string>();
            var votingIdVal = long.Parse(votingId);
            foreach (var token in tokens)
            {
                var index = (int)token["Index"];
                var tokenValue = (string) token["Token"];
                signedTokens.Add(_votingTokenHandler.Sign(tokenValue, votingIdVal, index));
            }

            var registrationId = Guid.NewGuid().ToString();
            

            // TODO: save signed tokens for voting

            return new JsonResult(new
            {
                SignedTokens = signedTokens
            });
        }

        public IActionResult HandySignatur()
        {
            return View();
        }
    }
}
