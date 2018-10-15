﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Application.VotingResults;
using FreieWahl.Common;
using FreieWahl.Models.Voting;
using FreieWahl.Models.VotingAdministration;
using FreieWahl.Security.Signing.VotingTokens;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FreieWahl.Controllers
{
    public class VotingController : Controller
    {
        private readonly IVotingTokenHandler _votingTokenHandler;
        private readonly IVotingStore _votingStore;
        private readonly IVotingResultManager _votingResultManager;
        private readonly string _regUrl;

        public VotingController(
            IVotingTokenHandler votingTokenHandler,
            IVotingStore votingStore,
            IConfiguration configuration,
            IVotingResultManager votingResultManager)
        {
            _votingTokenHandler = votingTokenHandler;
            _votingStore = votingStore;
            _votingResultManager = votingResultManager;

            _regUrl = configuration["RemoteTokenStore:Url"];
        }

        public async Task<IActionResult> Register(string votingId)
        {
            var id = votingId.ToId();
            if (!id.HasValue)
                return BadRequest("Invalid votingId");

            var voting = await _votingStore.GetById(id.Value);
            ViewData["VotingId"] = votingId;
            ViewData["RegistrationStoreId"] = Guid.NewGuid().ToString("D");
            ViewData["VotingTitle"] = voting.Title;
            ViewData["VotingDescription"] = voting.Description;

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
                signedTokens[count] = _votingTokenHandler.Sign(token, votingIdVal, count);
                count++;
            }

            var result = new JsonResult(new
            {

            });

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> GetAnsweredQuestionIndices(string votingId, string[] tokens)
        {
            var id = votingId.ToId();
            if (!id.HasValue)
                return BadRequest("Invalid votingId");

            var votes = await _votingResultManager.GetResults(id.Value, tokens);
            var result = new JsonResult(new
            {
                AnsweredIndices = votes.Select(x => x.QuestionIndex).ToArray()
            });
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Vote(string votingId, string voterId)
        {
            var id = votingId.ToId();
            if (id.HasValue == false)
                return BadRequest("Invalid voting id");

            var voting = await _votingStore.GetById(id.Value);
            if (voting == null)
            {
                return BadRequest("No voting with the given id");
            }

            var questions = voting.Questions
                .Where(x => x.Status == QuestionStatus.OpenForVoting)
                .Select(x => new QuestionModel(x, votingId)).ToArray();

            var model = new VoteModel
            {
                StartDate = voting.StartDate,
                EndDate = voting.EndDate,
                VotingTitle = voting.Title,
                VotingId = votingId,
                VoterId = voterId,
                Questions = questions,
                GetTokensUrl = _regUrl + "getTokens"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetVotingQuestions(string votingId, string voterId)
        {
            var id = votingId.ToId();
            if (!id.HasValue)
            {
                return BadRequest("Invalid voting id!");
            }

            var voting = await _votingStore.GetById(id.Value);
            if (voting == null)
            {
                return BadRequest("No voting with the given id");
            }

            var questions = voting.Questions
                .Where(x => x.Status == QuestionStatus.OpenForVoting)
                .Select(x => new QuestionModel(x, votingId)).ToArray();
            // TODO
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> CastVote(string votingId, string voterId, int questionIndex)
        {
            var id = votingId.ToId();
            if (id.HasValue == false)
                return BadRequest("Invalid voting id");

            var voting = await _votingStore.GetById(id.Value);
            if (voting == null)
            {
                return BadRequest("No voting with the given id");
            }

            var question = voting.Questions.FirstOrDefault(x =>
                x.Status == QuestionStatus.OpenForVoting && x.QuestionIndex == questionIndex);
            if (question == null)
            {
                return BadRequest("No question with the given id is ready for voting");
            }

            var model = question.AnswerOptions.Select(x => new FreieWahl.Models.Voting.AnswerOption
            {
                Text = x.AnswerText,
                Description = "yadda yadda ", // TODO
                Id = x.Id
            });
            ViewData["VoterId"] = voterId;
            ViewData["VotingId"] = votingId;
            ViewData["QuestionIndex"] = questionIndex;
            ViewData["RegistrationStoreGetTokenUrl"] = _regUrl + "getToken";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitVote(string votingId, int questionIndex, string[] answerIds,
            string signedToken, string token)
        {
            var votingIdVal = long.Parse(votingId);
            await _votingResultManager.StoreVote(votingIdVal, questionIndex, answerIds.ToList(), token,
                signedToken);
            return Ok();
        }
    }
}
