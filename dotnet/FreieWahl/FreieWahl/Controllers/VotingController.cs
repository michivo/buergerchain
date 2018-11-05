﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Application.Voting;
using FreieWahl.Application.VotingResults;
using FreieWahl.Common;
using FreieWahl.Helpers;
using FreieWahl.Models.Voting;
using FreieWahl.Models.VotingAdministration;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FreieWahl.Controllers
{
    [ForwardedRequireHttps]
    public class VotingController : Controller
    {
        private readonly IVotingManager _votingManager;
        private readonly IVotingResultManager _votingResultManager;
        private readonly string _regUrl;

        public VotingController(
            IVotingManager votingManager,
            IConfiguration configuration,
            IVotingResultManager votingResultManager)
        {
            _votingManager = votingManager;
            _votingResultManager = votingResultManager;

            _regUrl = configuration["RemoteTokenStore:Url"];
        }

        public async Task<IActionResult> Register(string votingId)
        {
            var voting = await _votingManager.GetById(votingId);
            ViewData["VotingId"] = votingId;
            ViewData["RegistrationStoreId"] = Guid.NewGuid().ToString("D");
            ViewData["VotingTitle"] = voting.Title;
            ViewData["VotingDescription"] = voting.Description;
            ViewData["ImageData"] = voting.ImageData ?? string.Empty;
            ViewData["StartDate"] = voting.StartDate.ToSecondsSinceEpoch();
            ViewData["EndDate"] = voting.EndDate.ToSecondsSinceEpoch();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Vote(string votingId, string voterId)
        {
            var voting = await _votingManager.GetById(votingId);
            if (voting == null)
            {
                return BadRequest("No voting with the given id");
            }

            if (voting.State == VotingState.Closed)
            {
                return BadRequest("Voting has already been closed!");
            }

            var questions = voting.Questions
                .Where(x => x.Status == QuestionStatus.OpenForVoting)
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
                return BadRequest("No voting with the given id");
            }

            var votes = await _votingResultManager.GetResults(votingId, new[] { token });
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
                return BadRequest("No voting with the given id");
            }

            var votes = await _votingResultManager.GetResults(votingId, tokens);

            var questions = voting.Questions
                .Where(x => x.Status == QuestionStatus.OpenForVoting)
                .Select(x => new QuestionModel(x, votingId)).ToArray();

            var model = new QuestionData
            {
                Questions = questions.Select(x => _MapToModel(x, votes)).ToArray()
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
                return BadRequest("There is no voting with the given id");
            }
            if (voting.State == VotingState.Closed)
            {
                return BadRequest("Voting has already been closed!");
            }

            var question = voting.Questions.SingleOrDefault(x => x.QuestionIndex == questionIndex);
            if (question == null)
            {
                return BadRequest("There is no question with the given index!");
            }

            if (question.Status != QuestionStatus.OpenForVoting)
            {
                return BadRequest("The given question is no open for voting!");
            }

            try
            {
                await _votingResultManager.StoreVote(votingId, questionIndex, answerIds.ToList(), token,
                    signedToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Voting token could not be verified or vote could not be stored.");
            }
        }
    }
}
