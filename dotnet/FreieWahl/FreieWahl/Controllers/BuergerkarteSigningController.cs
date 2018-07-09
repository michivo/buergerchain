using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using FreieWahl.Security.Signing.VotingTokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FreieWahl.Security.Signing.Buergerkarte
{
    public class BuergerkarteSigningController : Controller
    {
        private readonly ISignatureHandler _signatureHandler;
        private readonly IVotingTokenSigning _votingTokenSigning;
        private string _lastSigResult;

        public BuergerkarteSigningController(ISignatureHandler signatureHandler,
            IVotingTokenSigning votingTokenSigning)
        {
            _signatureHandler = signatureHandler;
            _votingTokenSigning = votingTokenSigning;
        }

        [HttpPost]
        public IActionResult SignatureDataUrl()
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
            // TODO: verify that voter is registered for voting
            var tokens = (JArray)jObject.GetValue("Tokens", StringComparison.OrdinalIgnoreCase);
            var signedTokens = new List<string>();
            foreach (var token in tokens)
            {
                signedTokens.Add(_votingTokenSigning.Sign((string) token));
            }

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
