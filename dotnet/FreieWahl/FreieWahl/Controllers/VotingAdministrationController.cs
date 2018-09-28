using FreieWahl.Application.Authentication;
using FreieWahl.Models.VotingAdministration;
using FreieWahl.Security.UserHandling;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Application.Registrations;
using FreieWahl.Mail;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.Signing.VotingTokens;
using FreieWahl.UserData.Store;
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
        private readonly IUserDataStore _userDataStore;

        public VotingAdministrationController(
            IVotingStore votingStore,
            IStringLocalizer<VotingAdministrationController> localizer,
            IMailProvider mailProvider,
            IVotingTokenHandler tokenHandler, 
            IAuthorizationHandler authorizationHandler,
            IRemoteTokenStore remoteTokenStore, IUserDataStore userDataStore)
        {
            _votingStore = votingStore;
            _localizer = localizer;
            _mailProvider = mailProvider;
            _tokenHandler = tokenHandler;
            _authorizationHandler = authorizationHandler;
            _remoteTokenStore = remoteTokenStore;
            _userDataStore = userDataStore;
        }

        public async Task<IActionResult> Overview()
        {
            var user = await _GetUserForGetRequest();

            if (user == null)
                return Unauthorized();

            var img = await _userDataStore.GetUserImage(user.UserId);

            var model = new VotingOverviewModel
            {
                Image = img,
                FullName = user.Name,
                Initials = _GetInitials(user.Name)
            };
            return View(model);
        }

        private async Task<UserInformation> _GetUserForGetRequest()
        {
            var user = await _authorizationHandler.GetAuthorizedUser
                (null, Operation.List, Request.Cookies["token"]);
            return user;
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
            var user = await _authorizationHandler.GetAuthorizedUser
                (null, Operation.List, Request.Headers["Authorization"]);
            if (user == null)
                return Unauthorized();
            
            var votingsForUser = await _votingStore.GetForUserId(user.UserId);
            var resultForSerialization = votingsForUser.Select(x => new
            {
                x.Title,
                Description = x.Description ?? string.Empty,
                x.Id,
                x.ImageData
            });
            return new JsonResult(resultForSerialization.ToArray());
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

        [HttpPost]
        public async Task<IActionResult> SessionLogin(string idToken, string csrfToken)
        {
            var cookieToken = Request.Cookies["csrfToken"];
            if (cookieToken != csrfToken)
                return Unauthorized();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserImage(string imageData)
        {
            var user = await _authorizationHandler.GetAuthorizedUser(string.Empty, Operation.EditUser,
                Request.Headers["Authorization"]);
            if (user == null)
                return Unauthorized();

            await _userDataStore.SaveUserImage(user.UserId, imageData);

            return Ok();
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
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _GetUserForGetRequest();

            if (user == null)
                return Unauthorized();

            var voting = await _votingStore.GetById(_GetId(id));

            return View(new EditVotingModel
            {
                VotingId = id,
                Title = voting.Title,
                Description = voting.Description,
                ImageData = voting.ImageData,
                Questions = voting.Questions.Select(q => new QuestionModel(q)).ToList(),
                UserInitials = _GetInitials(user.Name),
                StartDate = voting.StartDate.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                EndDate = voting.EndDate.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                RegistrationUrl = _GetRegistrationUrl(id)
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVoting(string id, string title, string desc, string imageData,
            string startDate, string startTime, string endDate, string endTime)
        {
            var idVal = _GetId(id);
            var operation = idVal == 0 ? Operation.Create : Operation.UpdateVoting;
            UserInformation user = await
                _authorizationHandler.GetAuthorizedUser(id, operation, Request.Headers["Authorization"]);
            if (user == null)
                return Unauthorized();

            var startTimeValue = _GetDateTime(startDate, startTime);
            var endTimeValue = _GetDateTime(endDate, endTime);

            if (idVal != 0)
            {
                return await _UpdateVoting(id, title, desc, imageData);
            }

            return await _InsertVoting(title, desc, user, imageData, startTimeValue, endTimeValue);
        }

        private DateTime _GetDateTime(string date, string time)
        {
            if (!string.IsNullOrEmpty(time) && time.Count(x => x == ':') == 1)
                time += ":00";
            var result = DateTime.Parse(date + " " + time);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVotingQuestion(string id, string qid, string title, string desc, int type, string[] answers, string[] answerDescriptions,
            int minNumAnswers, int maxNumAnswers)
        {
            if (answers.Length != answerDescriptions.Length)
                return BadRequest("Number of Answers and Descriptions do not match");
            if (type < 1 || type > 3)
                return BadRequest("Invalid Question type, only decision, multiple choice and ordering are supported");
            if ((QuestionType) type != QuestionType.Decision && minNumAnswers > maxNumAnswers || maxNumAnswers > answers.Length ||
                minNumAnswers < 0)
                return BadRequest("Invalid numer of min/max number of answers");


            if (await _authorizationHandler.CheckAuthorization(id, Operation.UpdateQuestion, Request.Headers["Authorization"]) == false)
                return Unauthorized();

            var voting = await _votingStore.GetById(_GetId(id)); // TODO - handle missing voting
            var question = _GetQuestion(title, desc, answers, answerDescriptions);
            question.QuestionIndex = (int)_GetId(qid); // TODO: is cast ok here?
            question.QuestionType = (QuestionType) type;
            question.MinNumAnswers = question.QuestionType == QuestionType.Decision ? 1 : minNumAnswers;
            question.MaxNumAnswers = question.QuestionType == QuestionType.Decision ? 1 : maxNumAnswers;
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
            DateTime startDate, DateTime endDate)
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
                State = VotingState.InPreparation,
                ImageData = imageData,
                StartDate = startDate,
                EndDate = endDate
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

        private async Task<IActionResult> _UpdateVoting(string id, string title, string desc, string imageData)
        {
            var voting = await _votingStore.GetById(_GetId(id));
            voting.Title = title;
            voting.Description = desc;
            if (!string.IsNullOrEmpty(imageData))
                voting.ImageData = imageData;

            await _votingStore.Update(voting).ConfigureAwait(false);

            return Ok();
        }
    }
}