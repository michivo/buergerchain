using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Application.VotingResults;
using FreieWahl.Common;
using FreieWahl.Models.Voting;
using FreieWahl.Models.VotingAdministration;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FreieWahl.Controllers
{
    public class VotingController : Controller
    {
        private readonly IVotingStore _votingStore;
        private readonly IVotingResultManager _votingResultManager;
        private readonly string _regUrl;

        public VotingController(
            IVotingStore votingStore,
            IConfiguration configuration,
            IVotingResultManager votingResultManager)
        {
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
            ViewData["ImageData"] = voting.ImageData ?? string.Empty;
            ViewData["StartDate"] = voting.StartDate.ToMillisecondsSinceEpoch();
            ViewData["EndDate"] = voting.EndDate.ToMillisecondsSinceEpoch();

            return View();
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
                StartDate = voting.StartDate.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                EndDate = voting.EndDate.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
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

            var votes = await _votingResultManager.GetResults(id.Value, new[] { token });
            var vote = votes.SingleOrDefault();
            var answerIds = vote?.SelectedAnswerIds ?? new List<string>();

            var question = voting.Questions
                .SingleOrDefault(x => x.QuestionIndex == questionIndex);
            if (question == null)
            {
                return BadRequest("No answer available for the given question");
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


        public async Task<IActionResult> GetQuestions(string votingId, string voterId, string[] tokens)
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

            var votes = await _votingResultManager.GetResults(id.Value, tokens);

            var questions = voting.Questions
                .Where(x => x.Status == QuestionStatus.OpenForVoting)
                .Select(x => new QuestionModel(x, votingId)).ToArray();

            var model = new QuestionData
            {
                Questions = questions.Select(x => _MapToModel(x, votes)).ToArray()
            };

            return PartialView(model);
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
