﻿using FreieWahl.Application.Authentication;
using FreieWahl.Models.VotingAdministration;
using FreieWahl.Security.UserHandling;
using FreieWahl.Voting.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Application.Registrations;
using FreieWahl.Application.Tracking;
using FreieWahl.Application.Voting;
using FreieWahl.Application.VotingResults;
using FreieWahl.Common;
using FreieWahl.Helpers;
using FreieWahl.Mail;
using FreieWahl.Security.Signing.VotingTokens;
using FreieWahl.UserData.Store;
using FreieWahl.Voting.Common;
using Microsoft.AspNetCore.Http;

namespace FreieWahl.Controllers
{
    [ForwardedRequireHttps] 
    public class VotingAdministrationController : Controller
    {
        private readonly IVotingManager _votingManager;
        private readonly IMailProvider _mailProvider;
        private readonly IVotingTokenHandler _tokenHandler;
        private readonly IAuthorizationHandler _authorizationHandler;
        private readonly IRemoteTokenStore _remoteTokenStore;
        private readonly IUserDataStore _userDataStore;
        private readonly IVotingResultManager _votingResults;


        public VotingAdministrationController(
            IVotingManager votingManager,
            IMailProvider mailProvider,
            IVotingTokenHandler tokenHandler, 
            IAuthorizationHandler authorizationHandler,
            IRemoteTokenStore remoteTokenStore, 
            IUserDataStore userDataStore,
            IVotingResultManager votingResults)
        {
            _votingManager = votingManager;
            _mailProvider = mailProvider;
            _tokenHandler = tokenHandler;
            _authorizationHandler = authorizationHandler;
            _remoteTokenStore = remoteTokenStore;
            _userDataStore = userDataStore;
            _votingResults = votingResults;
        }

        public async Task<IActionResult> Overview()
        {
            var auth = await _GetUserForGetRequest(Operation.List);

            if (auth.IsAuthorized == false)
                return Unauthorized();

            var img = await _userDataStore.GetUserImage(auth.User.UserId);

            var model = new VotingOverviewModel
            {
                Image = img,
                FullName = auth.User.Name,
                Initials = _GetInitials(auth.User.Name)
            };
            return View(model);
        }

        private Task<AuthenticationResult> _GetUserForGetRequest(Operation operation, string id = null)
        {
            return _authorizationHandler.CheckAuthorization(id, operation, Request.Cookies["session"]);
        }

        private string _GetInitials(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return string.Empty;
            var nameParts = userName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length == 0)
                return string.Empty;
            if (nameParts.Length == 1)
                return nameParts[0][0].ToString();

            return nameParts[0][0].ToString() + nameParts[nameParts.Length - 1][0];
        }

        public async Task<IActionResult> GetVotingsForUser()
        {
            var auth = await _authorizationHandler.CheckAuthorization
                (null, Operation.List, Request.Cookies["session"]);
            if (auth.IsAuthorized == false)
                return Unauthorized();
            
            var votingsForUser = await _votingManager.GetForUserId(auth.User.UserId);
            var resultForSerialization = votingsForUser.Select(x => new
            {
                x.Title,
                Description = x.Description ?? string.Empty,
                x.Id,
                x.ImageData,
                Status = (int)x.State
            });
            return new JsonResult(resultForSerialization.ToArray());
        }

        [HttpPost]
        public async Task<IActionResult> UnlockQuestion(string votingId, int questionIndex)
        {
            var auth = await _authorizationHandler.CheckAuthorization(votingId, Operation.UpdateQuestion, Request.Cookies["session"]);
            if (auth.IsAuthorized == false || auth.Voting == null)
                return Unauthorized();

            var voting = auth.Voting;
            var question = voting.Questions.SingleOrDefault(x => x.QuestionIndex == questionIndex);

            if (question == null || question.Status != QuestionStatus.InPreparation)
                return BadRequest();

            question.Status = QuestionStatus.OpenForVoting;
            await _votingManager.UpdateQuestion(votingId, question);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> LockQuestion(string votingId, int questionIndex)
        {
            var auth = await _authorizationHandler.CheckAuthorization(votingId, Operation.UpdateQuestion, Request.Cookies["session"]);
            if (!auth.IsAuthorized || auth.Voting == null)
                return Unauthorized();

            var voting = auth.Voting;
            var question = voting.Questions.SingleOrDefault(x => x.QuestionIndex == questionIndex);

            if (question == null || question.Status != QuestionStatus.OpenForVoting)
                return BadRequest();

            question.Status = QuestionStatus.Locked;
            await _votingManager.UpdateQuestion(votingId, question);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserImage(string imageData)
        {
            var auth = await _authorizationHandler.CheckAuthorization(string.Empty, Operation.EditUser,
                Request.Cookies["session"]);
            if (auth.IsAuthorized == false)
                return Unauthorized();

            await _userDataStore.SaveUserImage(auth.User.UserId, imageData);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id, bool isNew = false)
        {
            var auth = await _GetUserForGetRequest(Operation.UpdateVoting, id);

            if (auth.IsAuthorized == false || auth.Voting == null)
                return Unauthorized();

            var voting = auth.Voting;
            List<Vote> votes = null;
            if (voting.Questions.Any(x => x.Status == QuestionStatus.Locked))
            {
                votes = (await _votingResults.GetResults(id)).ToList();
            }

            return View(new EditVotingModel
            {
                VotingId = id,
                Title = voting.Title,
                Description = voting.Description,
                ImageData = voting.ImageData,
                Questions = voting.Questions.Select(q => _CreateQuestionModel(q, id, votes)).ToList(),
                UserInitials = _GetInitials(auth.User.Name),
                StartDate = voting.StartDate.ToSecondsSinceEpoch(),
                EndDate = voting.EndDate.ToSecondsSinceEpoch(),
                RegistrationUrl = _GetRegistrationUrl(id),
                IsNew = isNew
            });
        }


        [HttpGet]
        public async Task<IActionResult> QuestionList(string id)
        {
            var auth = await _GetUserForGetRequest(Operation.UpdateQuestion, id);

            if (auth.IsAuthorized == false)
                return Unauthorized();

            var voting = auth.Voting;
            List<Vote> votes = null;
            if (voting.Questions.Any(x => x.Status == QuestionStatus.Locked))
            {
                votes = (await _votingResults.GetResults(id)).ToList();
            }

            return PartialView(new EditVotingModel
            {
                VotingId = id,
                Title = voting.Title,
                Description = voting.Description,
                ImageData = voting.ImageData,
                Questions = voting.Questions.Select(x => _CreateQuestionModel(x, id, votes)).ToList(),
                UserInitials = _GetInitials(auth.User.Name),
                StartDate = voting.StartDate.ToSecondsSinceEpoch(),
                EndDate = voting.EndDate.ToSecondsSinceEpoch(),
                RegistrationUrl = _GetRegistrationUrl(id)
            });
        }

        [HttpGet]
        public async Task<IActionResult> ResultQuestionList(string id)
        {
            var auth = await _GetUserForGetRequest(Operation.UpdateQuestion, id);

            if (auth.IsAuthorized == false)
                return Unauthorized();

            var voting = auth.Voting;
            List<Vote> votes = null;
            if (voting.Questions.Any(x => x.Status == QuestionStatus.Locked))
            {
                votes = (await _votingResults.GetResults(id)).ToList();
            }

            return PartialView(new VotingResultsModel
            {
                VotingId = id,
                Title = voting.Title,
                Description = voting.Description,
                ImageData = voting.ImageData,
                Questions = voting.Questions.Select(x => _CreateQuestionModel(x, id, votes)).ToList(),
                UserInitials = _GetInitials(auth.User.Name),
                StartDate = voting.StartDate.ToSecondsSinceEpoch(),
                EndDate = voting.EndDate.ToSecondsSinceEpoch()
            });
        }


        [HttpGet]
        public async Task<IActionResult> Results(string id)
        {
            var auth = await _GetUserForGetRequest(Operation.UpdateQuestion, id);

            if (auth.IsAuthorized == false)
                return Unauthorized();

            var voting = auth.Voting;
            List<Vote> votes = null;
            if (voting.Questions.Any(x => x.Status == QuestionStatus.Locked))
            {
                votes = (await _votingResults.GetResults(id)).ToList();
            }

            return View(new VotingResultsModel
            {
                VotingId = id,
                Title = voting.Title,
                Description = voting.Description,
                ImageData = voting.ImageData,
                Questions = voting.Questions.Select(q => _CreateQuestionModel(q, id, votes)).ToList(),
                UserInitials = _GetInitials(auth.User.Name),
                StartDate = voting.StartDate.ToSecondsSinceEpoch(),
                EndDate = voting.EndDate.ToSecondsSinceEpoch(),
            });
        }

        private static QuestionModel _CreateQuestionModel(Question q, string id, List<Vote> votes)
        {
            if(q.Status != QuestionStatus.Locked)
                return new QuestionModel(q, id, new List<List<string>>());

            return new QuestionModel(q, id, 
                votes.Where(x => x.QuestionIndex == q.QuestionIndex)
                    .Select(x => x.SelectedAnswerIds)
                    .ToList());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVoting(string id, string title, string desc, string imageData,
            string startDate, string endDate, int regType)
        {
            var operation = string.IsNullOrEmpty(id) ? Operation.Create : Operation.UpdateVoting;
            var auth = await
                _authorizationHandler.CheckAuthorization(id, operation, Request.Cookies["session"]);
            if (auth.IsAuthorized == false)
                return Unauthorized();

            var startTimeValue = DateTime.Parse(startDate, null, DateTimeStyles.RoundtripKind);
            var endTimeValue = DateTime.Parse(endDate, null, DateTimeStyles.RoundtripKind);
            if (regType < 1 || regType > 3)
            {
                return BadRequest();
            }

            RegistrationType registrationType = (RegistrationType)regType;

            if (!string.IsNullOrEmpty(id))
            {
                return await _UpdateVoting(id, title, desc, imageData, startTimeValue, endTimeValue, registrationType);
            }

            return await _InsertVoting(title, desc, auth.User, imageData, startTimeValue, endTimeValue, registrationType);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVotingQuestion(string id, int qid, string title, string desc, int type, string[] answers, string[] answerDescriptions,
            int minNumAnswers, int maxNumAnswers)
        {
            if (answers.Length != answerDescriptions.Length)
                return BadRequest("Number of Answers and Descriptions do not match");
            if (type < 1 || type > 3)
                return BadRequest("Invalid Question type, only decision, multiple choice and ordering are supported");
            if ((QuestionType) type != QuestionType.Decision && (minNumAnswers > maxNumAnswers || minNumAnswers < 0))
                return BadRequest("Invalid numer of min/max number of answers");

            var auth = await _authorizationHandler.CheckAuthorization(id, Operation.UpdateQuestion, Request.Cookies["session"]);
            if (auth.IsAuthorized == false || auth.Voting == null)
                return Unauthorized();

            var voting = auth.Voting;
            var question = _GetQuestion(title, desc, answers, answerDescriptions);
            if (question.Status != QuestionStatus.InPreparation)
            {
                return BadRequest("The answer has already been locked!");
            }

            question.QuestionIndex = qid;
            question.QuestionType = (QuestionType) type;
            question.MinNumAnswers = question.QuestionType == QuestionType.Decision ? 1 : minNumAnswers;
            question.MaxNumAnswers = question.QuestionType == QuestionType.Decision ? 1 : maxNumAnswers;
            if (question.QuestionIndex == 0)
            {
                await _votingManager.AddQuestion(voting.Id, question);
                return Ok();
            }

            await _votingManager.UpdateQuestion(voting.Id, question);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVotingQuestion(string id, string qid)
        {
            if ((await _authorizationHandler.CheckAuthorization(id, Operation.UpdateQuestion, Request.Cookies["session"])).IsAuthorized == false)
                return Unauthorized();
            var questionId = qid.ToId();
            if (!questionId.HasValue)
                return BadRequest("Invalid votingId/questionid");

            await _votingManager.DeleteQuestion(id, (int)questionId.Value);
            return Ok(); 
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVoting(string id)
        {
            if ((await _authorizationHandler.CheckAuthorization(id, Operation.DeleteVoting, Request.Cookies["session"])).IsAuthorized == false)
                return Unauthorized();

            await _votingManager.Delete(id);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendInvitationMail(string votingId, string[] addresses)
        {
            if ((await _authorizationHandler.CheckAuthorization(votingId, Operation.Invite, Request.Cookies["session"])).IsAuthorized == false)
                return Unauthorized();

            var voting = await _votingManager.GetById(votingId);
            var registrationUrl = _GetRegistrationUrl(votingId);
            var registrationUrlLink = "<a href=\"" + registrationUrl + "\">" + registrationUrl + "</a>";
            var dateString = voting.StartDate.ToString("HH:mm") + " am " + voting.StartDate.ToString("dd.MM.yyyy");
            var mailText = $"Liebe Wahlberechtigte!<br/>Sie sind eingeladen, an unserer Abstimmung <i>{voting.Title}</i> teilzunehmen.Bitte registrieren Sie sich bis spätestens {dateString} unter %link%.<br />Vielen Dank!";
            var subject = "Einladung zur Abstimmung " + voting.Title;

            await _mailProvider.SendMail(new List<string>(addresses), subject, mailText,
                new Dictionary<string, string> {{"%link%", registrationUrlLink} });
            return Ok();
        }

        private string _GetRegistrationUrl(string votingId)
        {
            var result = Url.Action("Register", "Voting", new {votingId}, HttpContext.Request.Scheme);
            if (result.StartsWith("http://") && !result.StartsWith("http://localhost"))
            { // TODO this is not clean
                result = "https" + result.Substring(4);
            }

            return result;
        }

        private static Question _GetQuestion(string title, string desc, string[] answers, string[] answerDescriptions)
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
            for(int i = 0; i < answers.Length; i++)
            {
                if (string.IsNullOrEmpty(answers[i]))
                    continue;
                answerOptions.Add(new AnswerOption
                {
                    AnswerText = answers[i],
                    Details = new List<AnswerDetail>
                    {
                        new AnswerDetail { DetailType = AnswerDetailType.AdditionalInfo, DetailValue = answerDescriptions[i] }
                    }
                });
            }

            var question = new Question
            {
                AnswerOptions = answerOptions,
                QuestionText = title ?? "---",
                Status = QuestionStatus.InPreparation,
                Details = details
            };
            return question;
        }

        private async Task<IActionResult> _InsertVoting(string title, string desc, UserInformation user, string imageData,
            DateTime startDate, DateTime endDate, RegistrationType registrationType)
        {
            if ((await _authorizationHandler.CheckAuthorization(null, Operation.Create, Request.Cookies["session"])).IsAuthorized == false)
                return Unauthorized();

            StandardVoting voting = new StandardVoting()
            {
                Creator = user.UserId,
                DateCreated = DateTime.UtcNow,
                Description = desc,
                Questions = new List<Question>(),
                Title = title,
                Visibility = VotingVisibility.OwnerOnly,
                State = VotingState.InPreparation,
                ImageData = imageData,
                StartDate = startDate,
                EndDate = endDate,
                SupportedRegistrationType = registrationType,
                CurrentQuestionIndex = 0
            };

            await _votingManager.Insert(voting).ConfigureAwait(false);

            // not awaited intentionally! this might take a few minutes, we do not need to wait for it to finish
#pragma warning disable 4014
            _tokenHandler.GenerateTokens(voting.Id).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully) return;

                    var keys = t.Result;
                    _remoteTokenStore.InsertPublicKeys(voting.Id, keys);
                    _votingManager.UpdateState(voting.Id, VotingState.Ready);
                });
#pragma warning restore 4014

            return Ok(voting.Id.ToString(CultureInfo.InvariantCulture));
        }

        private async Task<IActionResult> _UpdateVoting(string id, string title, string desc, string imageData,
            DateTime startDate, DateTime endDate, RegistrationType registrationType)
        {
            var voting = await _votingManager.GetById(id);
            voting.Title = title;
            voting.Description = desc;
            voting.StartDate = startDate;
            voting.EndDate = endDate;
            voting.SupportedRegistrationType = registrationType;

            if (!string.IsNullOrEmpty(imageData))
                voting.ImageData = imageData;

            await _votingManager.Update(voting).ConfigureAwait(false);

            return Ok();
        }
    }
}