using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Application.Voting;
using FreieWahl.Application.VotingResults;
using FreieWahl.Common;
using FreieWahl.Helpers;
using FreieWahl.Models.Voting;
using FreieWahl.Models.VotingAdministration;
using FreieWahl.Voting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FreieWahl.Controllers
{
    [ForwardedRequireHttps]
    public class VotingController : Controller
    {
        private readonly IVotingManager _votingManager;
        private readonly IVotingResultManager _votingResultManager;
        private readonly IVotingChainBuilder _votingChainBuilder;
        private readonly string _regUrl;

        public VotingController(
            IVotingManager votingManager,
            IConfiguration configuration,
            IVotingResultManager votingResultManager,
            IVotingChainBuilder votingChainBuilder)
        {
            _votingManager = votingManager;
            _votingResultManager = votingResultManager;
            _votingChainBuilder = votingChainBuilder;

            _regUrl = configuration["RemoteTokenStore:Url"];
        }

        public async Task<IActionResult> Register(string votingId)
        {
            var voting = await _votingManager.GetById(votingId);
            if (voting == null)
            {
                return BadRequest("Ungültige Abstimmungs-ID " + votingId);
            }
            ViewData["VotingId"] = votingId;
            ViewData["RegistrationStoreId"] = Guid.NewGuid().ToString("D");
            ViewData["VotingTitle"] = voting.Title;
            ViewData["VotingDescription"] = voting.Description;
            ViewData["ImageData"] = voting.ImageData ?? string.Empty;
            ViewData["StartDate"] = voting.StartDate.ToSecondsSinceEpoch();
            ViewData["EndDate"] = voting.EndDate.ToSecondsSinceEpoch();
            ViewData["VotingState"] = voting.State;
            ViewData["RegistrationType"] = voting.SupportedRegistrationType;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Vote(string votingId, string voterId)
        {
            var voting = await _votingManager.GetById(votingId);
            if (voting == null)
            {
                return BadRequest("Die Abstimmung existiert nicht.");
            }

            if (DateTime.UtcNow < voting.StartDate)
            {
                return BadRequest("In dieser Abstimmung kann noch nicht abgestimmt werden!");
            }

            var questions = voting.Questions
                .Where(x => x.Status == QuestionStatus.OpenForVoting || x.Status == QuestionStatus.Locked)
                .Select(x => new QuestionModel(x, votingId)).ToArray();

            var model = new VoteModel
            {
                StartDate = voting.StartDate.ToSecondsSinceEpoch(),
                EndDate = voting.EndDate.ToSecondsSinceEpoch(),
                VotingTitle = voting.Title,
                VotingId = votingId,
                VoterId = voterId,
                Questions = questions,
                GetTokensUrl = _regUrl + "getTokens",
                VotingDescription = voting.Description,
                ImageData = voting.ImageData,
                GetSignedTokenUrl = _regUrl + "getToken"
            };

            return View(model);
        }

        public async Task<IActionResult> GetQuestion(string votingId, string voterId, int questionIndex, string token)
        {
            var voting = await _votingManager.GetById(votingId);
            if (voting == null)
            {
                return BadRequest("Die Abstimmung existiert nicht.");
            }

            if (DateTime.UtcNow < voting.StartDate)
            {
                return BadRequest("In dieser Abstimmung kann noch nicht abgestimmt werden!");
            }

            var votes = await _votingResultManager.GetResults(votingId, new[] { token });
            var vote = votes.SingleOrDefault();
            var answerIds = vote?.SelectedAnswerIds ?? new List<string>();

            var question = voting.Questions
                .SingleOrDefault(x => x.QuestionIndex == questionIndex);
            if (question == null)
            {
                return BadRequest("Die Frage existiert nicht.");
            }

            var questionModel = new QuestionModel(question, votingId);
            var model = new VotingQuestionModel
            {
                Question = questionModel,
                AnswerStatus = vote == null ? VotingQuestionStatus.Open : VotingQuestionStatus.Answered,
                SelectedAnswerIds = answerIds.ToArray()
            };

            return PartialView(model);
        }

        public async Task<IActionResult> GetResults(string votingId, int questionIndex)
        {
            var results = await _votingResultManager.GetResults(votingId, questionIndex);
            var model = new QuestionResultModel()
            {
                SelectedAnswerIds = results.Select(x => x.SelectedAnswerIds.ToArray()).ToArray()
            };
            return PartialView(model);
        }

        public async Task<IActionResult> GetQuestions(string votingId, string voterId, string[] tokens)
        {
            var voting = await _votingManager.GetById(votingId);
            if (voting == null)
            {
                return BadRequest("Diese Abstimmung existiert nicht.");
            }

            if (DateTime.UtcNow < voting.StartDate)
            {
                return BadRequest("In dieser Abstimmung kann noch nicht abgestimmt werden!");
            }

            var votes = await _votingResultManager.GetResults(votingId, tokens);

            var questions = voting.Questions
                .Where(x => x.Status == QuestionStatus.OpenForVoting || x.Status == QuestionStatus.Locked)
                .Select(x => new QuestionModel(x, votingId)).ToArray();

            var model = new QuestionData
            {
                Questions = questions.Select(x => _MapToModel(x, votes)).ToArray(),
                DeadlinePassed = voting.State == VotingState.Closed
            };

            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetNumberOfAnswers(string votingId, int[] questionIndices)
        {
            var results = await _votingResultManager.GetResults(votingId);
            var answerCounts = results.Where(x => questionIndices.Contains(x.QuestionIndex))
                .GroupBy(x => x.QuestionIndex)
                .OrderBy(x => x.Key)
                .Select(y => new
                {
                    Index = y.Key,
                    Count = y.Count()
                })
                .ToArray();

            return new JsonResult(answerCounts);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyVotes(string votingId, int questionIndex)
        {
            var voting = await _votingManager.GetById(votingId).ConfigureAwait(false);
            var votes = await _votingResultManager.GetResults(votingId, questionIndex).ConfigureAwait(false);

            try
            {
                _votingChainBuilder.CheckChain(voting.Questions.Single(x => x.QuestionIndex == questionIndex),
                    votes.ToList());
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return Ok(votes.Count + 1);
        }

        private VotingQuestionModel _MapToModel(QuestionModel questionModel, IReadOnlyCollection<Vote> votes)
        {
            var matchingVotes = votes.Where(x => x.QuestionIndex == questionModel.Index).ToList();
            if (matchingVotes.Count > 1)
            {
                throw new ArgumentException("Invalid vote state, multiple answers by one voter!");
            }

            var answerState = matchingVotes.Count == 0 ? VotingQuestionStatus.Open : VotingQuestionStatus.Answered;
            var answers = matchingVotes.Count == 0 ? new string[0] : matchingVotes[0].SelectedAnswerIds.ToArray();

            return new VotingQuestionModel
            {
                Question = questionModel,
                AnswerStatus = answerState,
                SelectedAnswerIds = answers
            };
        }

        [HttpPost]
        public async Task<IActionResult> SubmitVote(string votingId, int questionIndex, string[] answerIds,
            string signedToken, string token)
        {
            var voting = await _votingManager.GetById(votingId);
            if (voting == null)
            {
                return BadRequest("Die Abstimmung existiert nicht!");
            }
            if (voting.State == VotingState.Closed || DateTime.UtcNow > voting.EndDate)
            {
                return BadRequest("In dieser Abstimmung kann nicht mehr abgestimmt werden!");
            }

            if (DateTime.UtcNow < voting.StartDate)
            {
                return BadRequest("In dieser Abstimmung kann noch nicht abgestimmt werden!");
            }

            var question = voting.Questions.SingleOrDefault(x => x.QuestionIndex == questionIndex);
            if (question == null)
            {
                return BadRequest("Ungültige Frage!");
            }

            if (question.Status != QuestionStatus.OpenForVoting)
            {
                return BadRequest("Diese Frage ist nicht zur Abstimmung freigegeben!");
            }

            try
            {
                await _votingResultManager.StoreVote(votingId, questionIndex, answerIds.ToList(), token,
                    signedToken);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Ungültige Stimmabgabe. Ihre Stimme konnte nicht gespeichert werden.");
            }
        }
    }
}
