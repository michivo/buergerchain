using System.Collections.Generic;
using FreieWahl.Security.Signing.VotingTokens;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FreieWahl.Controllers
{
    public class VotingController : Controller
    {
        private readonly IVotingTokenSigning _signing;
        private readonly IVotingTokenVerifier _verifier;

        public VotingController(
            IVotingTokenSigning signing,
            IVotingTokenVerifier verifier)
        {
            _signing = signing;
            _verifier = verifier;
        }

        // GET: /VotingController/
        public IActionResult SignTokens(string[] tokens, string signature)
        {
            
            var signedTokens = new string[tokens.Length];
            int count = 0;
            foreach (var token in tokens)
            {
                signedTokens[count++] = _signing.Sign(token);
            }

            var result = new JsonResult(new
            {

            });

            return result;
        }
    }
}
