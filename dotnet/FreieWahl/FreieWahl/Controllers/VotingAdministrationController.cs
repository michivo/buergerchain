using FreieWahl.Application.Authentication;
using FreieWahl.Models.VotingAdministration;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.UserHandling;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Application.Registrations;
using FreieWahl.Mail;
using FreieWahl.Security.Signing.VotingTokens;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace FreieWahl.Controllers
{
    public class VotingAdministrationController : Controller
    {
        private readonly IVotingStore _votingStore;
        private readonly IStringLocalizer<VotingAdministrationController> _localizer;
        private readonly IMailProvider _mailProvider;
        private readonly IVotingTokenHandler _tokenHandler;
        private readonly IAuthorizationHandler _authorizationHandler;
        private readonly IRemoteTokenStore _remoteTokenStore;

        public VotingAdministrationController(
            IVotingStore votingStore,
            IStringLocalizer<VotingAdministrationController> localizer,
            IMailProvider mailProvider,
            IVotingTokenHandler tokenHandler, 
            IAuthorizationHandler authorizationHandler,
            IRemoteTokenStore remoteTokenStore)
        {
            _votingStore = votingStore;
            _localizer = localizer;
            _mailProvider = mailProvider;
            _tokenHandler = tokenHandler;
            _authorizationHandler = authorizationHandler;
            _remoteTokenStore = remoteTokenStore;
        }

        public IActionResult Overview()
        {
            var model = new VotingOverviewModel
            {
                Title = _localizer["Title"],
                Header = _localizer["Header"]
            };
            return View(model);
        }

        public async Task<IActionResult> GetVotingsForUser()
        {
            var user = await _authorizationHandler.GetAuthorizedUser
                (null, Operation.List, Request.Headers["Authorization"]);
            if (user == null)
                return Unauthorized();
            
            var votingsForUser = await _votingStore.GetForUserId(user.UserId);
            var resultForSerialization = votingsForUser.Select(x => new
            {
                x.Title,
                Description = x.Description ?? string.Empty,
                x.Id
            });
            return new JsonResult(resultForSerialization.ToArray());
        }

        public async Task<IActionResult> GetVotingDetails(string id)
        {
            if (await _authorizationHandler.CheckAuthorization(id, Operation.Read, Request.Headers["Authorization"]) == false)
                return Unauthorized();

            var voting = await _votingStore.GetById(_GetId(id));
            var result = new
            {
                Id = voting.Id.ToString(CultureInfo.InvariantCulture),
                voting.Title,
                voting.Description,
                Questions = voting.Questions.Select(x =>
                    new
                    {
                        Text = x.QuestionText,
                        AnswerOptions = x.AnswerOptions.Select(a => a.AnswerText).ToArray(),
                        Id = x.QuestionIndex.ToString(CultureInfo.InvariantCulture)
                    }).ToArray()
            };

            return new JsonResult(result);
        }

        public async Task<IActionResult> GetQuestion(string votingId, string questionId)
        {
            if (await _authorizationHandler.CheckAuthorization(votingId, Operation.UpdateQuestion, Request.Headers["Authorization"]) == false)
                return Unauthorized();

            var voting = await _votingStore.GetById(_GetId(votingId));
            var qid = _GetId(questionId);
            var question = voting.Questions.SingleOrDefault(x => x.QuestionIndex == qid);
            if (question == null)
                return BadRequest("Invalid question id"); // TODO
            var result = new
            {
                Id = question.QuestionIndex,
                Text = question.QuestionText,
                Description = _GetDescription(question),
                AnswerOptions = question.AnswerOptions.Select(x =>
                    new
                    {
                        x.Id,
                        Text = x.AnswerText
                    })
            };
            return new JsonResult(result);
        }

        private string _GetDescription(Question question)
        {
            foreach (var questionDetail in question.Details)
            {
                if (questionDetail.DetailType == QuestionDetailType.AdditionalInfo)
                    return questionDetail.DetailValue;
            }

            return string.Empty;
        }

        private long _GetId(string s)
        {
            long result = 0;
            if (string.IsNullOrEmpty(s))
                return result;

            if (!long.TryParse(s, out result))
                return 0;

            return result;
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            return View(new EditVotingModel { VotingId = id });
        }

        [HttpGet]
        public IActionResult EditQuestion(string id, string qid)
        {
            return View(new EditVotingQuestionModel { VotingId = id, QuestionId = qid });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVoting(string id, string title, string desc)
        {
            var idVal = _GetId(id);
            var operation = idVal == 0 ? Operation.Create : Operation.UpdateVoting;
            UserInformation user = await
                _authorizationHandler.GetAuthorizedUser(id, operation, Request.Headers["Authorization"]);
            if (user == null)
                return Unauthorized();

            if (idVal != 0)
            {
                return await _UpdateVoting(id, title, desc);
            }

            return await _InsertVoting(title, desc, user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVotingQuestion(string id, string qid, string title, string desc, string[] answers)
        {
            if (await _authorizationHandler.CheckAuthorization(id, Operation.UpdateQuestion, Request.Headers["Authorization"]) == false)
                return Unauthorized();

            var voting = await _votingStore.GetById(_GetId(id)); // TODO - handle missing voting
            var question = _GetQuestion(title, desc, answers);
            question.QuestionIndex = (int)_GetId(qid); // TODO: is cast ok here?
            if (question.QuestionIndex == 0)
            {
                await _votingStore.AddQuestion(voting.Id, question);
                return Ok();
            }

            await _votingStore.UpdateQuestion(voting.Id, question);
            return Ok(); // TODO err handling
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVotingQuestion(string id, string qid)
        {
            if (await _authorizationHandler.CheckAuthorization(id, Operation.UpdateQuestion, Request.Headers["Authorization"]) == false)
                return Unauthorized();

            await _votingStore.DeleteQuestion(_GetId(id), (int)_GetId(qid));// TODO: is cast ok here?
            return Ok(); // TODO err handling
        }

        [HttpPost]
        public async Task<IActionResult> SendInvitationMail(string votingId, string[] addresses)
        {
            if (await _authorizationHandler.CheckAuthorization(votingId, Operation.Invite, Request.Headers["Authorization"]) == false)
                return Unauthorized();

            await _mailProvider.SendMail("Michael Faschinger", addresses[0], "Hello World", "This is just a -test-. Feel free to register for a voting <a href=\"-votingUrl-\">here</a> or copy the url to your browser: -votingUrl-.", 
                new Dictionary<string, string> {{"-test-", "surprise"}, {"-votingUrl-", _GetRegistrationUrl(votingId)}});
            return Ok();
        }

        private string _GetRegistrationUrl(string votingId)
        {
            var url = Request.GetUri();
            var baseUrl = url.GetLeftPart(UriPartial.Authority);
            var registrationUrl = baseUrl + "/Voting/Register?votingId=" + votingId;
            return registrationUrl;
        }

        private static Question _GetQuestion(string title, string desc, string[] answers)
        {
            var details = new List<QuestionDetail>();
            if (!string.IsNullOrEmpty(desc))
            {
                details.Add(new QuestionDetail
                {
                    DetailType = QuestionDetailType.AdditionalInfo,
                    DetailValue = desc
                });
            }

            List<AnswerOption> answerOptions = new List<AnswerOption>();
            foreach (var answer in answers)
            {
                answerOptions.Add(new AnswerOption
                {
                    AnswerText = answer,
                    Details = new List<AnswerDetail>()
                });
            }

            var question = new Question
            {
                AnswerOptions = answerOptions,
                QuestionText = title ?? "***---",
                Status = QuestionStatus.InPreparation,
                Details = details
            };
            return question;
        }

        private async Task<IActionResult> _InsertVoting(string title, string desc, UserInformation user)
        {
            if (await _authorizationHandler.CheckAuthorization(null, Operation.Create, Request.Headers["Authorization"]) == false)
                return Unauthorized();

            StandardVoting voting = new StandardVoting()
            {
                Creator = user.UserId,
                DateCreated = DateTime.UtcNow,
                Description = desc,
                Questions = new List<Question>(),
                Title = title,
                Visibility = VotingVisibility.OwnerOnly,
                State = VotingState.InPreparation
            };

            await _votingStore.Insert(voting).ConfigureAwait(false);

            // not awaited intentionally! this might take a few minutes, we do not need to wait for it to finish
#pragma warning disable 4014
            _tokenHandler.GenerateTokens(voting.Id).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully) return;

                    var keys = t.Result;
                    _remoteTokenStore.InsertPublicKeys(voting.Id, keys);
                    _votingStore.UpdateState(voting.Id, VotingState.Ready);
                });
#pragma warning restore 4014

            return Ok();
        }

        private async Task<IActionResult> _UpdateVoting(string id, string title, string desc)
        {
            var voting = await _votingStore.GetById(_GetId(id));
            voting.Title = title;
            voting.Description = desc;
            await _votingStore.Update(voting).ConfigureAwait(false);

            return Ok();
        }
    }
}