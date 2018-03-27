using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace FreieWahl.Security.Signing.Buergerkarte
{
    public class BuergerkarteSigningController : Controller
    {
        private string _lastSigResult;

        [HttpPost]
        public IActionResult SignatureDataUrl()
        {
            var data = Request.Form.Aggregate("", (x, y) => x += y.Key + " -> " + y.Value + " ... ");
            _lastSigResult = data;
            return new EmptyResult();
        }

        public IActionResult HandySignatur()
        {
            var uri = QueryHelpers.AddQueryString("https://www.a-trust.at/mobile/https-security-layer-request/default.aspx",
                new Dictionary<string, string>
                {
                    { "XMLRequest", "<?xml version='1.0' encoding='UTF-8'?>\n<sl:CreateCMSSignatureRequest xmlns:sl='http://www.buergerkarte.at/namespaces/securitylayer/1.2#' Structure='enveloping'>\n<sl:KeyboxIdentifier>SecureSignatureKeypair</sl:KeyboxIdentifier>\n<sl:DataObject>\n<sl:MetaInfo>\n<sl:MimeType>text/plain</sl:MimeType>\n</sl:MetaInfo>\n<sl:Content>\n<sl:Base64Content>SWNoIGJpbiBlaW4gZWluZmFjaGVyIFRleHQu</sl:Base64Content>\n</sl:Content>\n</sl:DataObject>\n</sl:CreateCMSSignatureRequest>"},
                    { "appletwidth", "250" },
                    { "appletheight", "250" },
                    { "DataURL", "https://stunning-lambda-162919.appspot.com/SignatureDataUrl" },
                    { "RedirectURL", "https://www.freiewahl.eu/Home/About" }
                });

            ViewData["IFrameURI"] = uri;
            return View();
        }
    }
}
