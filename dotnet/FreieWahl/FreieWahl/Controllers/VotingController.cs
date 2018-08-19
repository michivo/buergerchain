using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreieWahl.Security.Signing.VotingTokens;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FreieWahl.Controllers
{
    public class VotingController : Controller
    {
        private readonly IVotingTokenHandler _votingTokenHandler;

        public VotingController(
            IVotingTokenHandler votingTokenHandler)
        {
            _votingTokenHandler = votingTokenHandler;
        }

        public IActionResult Register(string votingId)
        {
            ViewData["VotingId"] = votingId;
            ViewData["RegistrationUid"] = Guid.NewGuid().ToString("D");
            return View();
        }

        // GET: /VotingController/
        public async Task<IActionResult> SignTokens(string votingId, string[] tokens, string signature)
        {   
            var signedTokens = new string[tokens.Length];
            int count = 0;
            var votingIdVal = long.Parse(votingId);
            foreach (var token in tokens)
            {
                signedTokens[count] = await _votingTokenHandler.Sign(token, votingIdVal, count);
                count++;
            }

            var result = new JsonResult(new
            {

            });

            return result;
        }

        public async Task<IActionResult> CastVote(string votingId, int questionIndex, List<string> answerId,
            string signedToken, string token)
        {
            var votingIdVal = long.Parse(votingId);
            if (await _votingTokenHandler.Verify(signedToken, token, votingIdVal, questionIndex) == false)
                return Unauthorized();

            // TODO: check that number of answers is valid


            return Ok();
        }
    }
}
