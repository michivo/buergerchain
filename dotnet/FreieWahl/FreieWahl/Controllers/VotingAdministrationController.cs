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

namespace FreieWahl.Controllers
{
    public class VotingAdministrationController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVotingStore _votingStore;
        private readonly IStringLocalizer<VotingAdministrationController> _localizer;
        private readonly IJwtAuthentication _authentication;
        private readonly IUserHandler _userHandler;
        private readonly IAuthenticationManager _authManager;
        private UserInformation _user;

        public VotingAdministrationController(ILogger<HomeController> logger,
            IVotingStore votingStore,
            IStringLocalizer<VotingAdministrationController> localizer,
            IJwtAuthentication authentication,
            IUserHandler userHandler,
            IAuthenticationManager authManager)
        {
            _logger = logger;
            _votingStore = votingStore;
            _localizer = localizer;
            _authentication = authentication;
            _userHandler = userHandler;
            _authManager = authManager;
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
            var result = _authentication.CheckToken(Request.Headers["Authorization"]);
            if (!result.Success)
                return Unauthorized();

            UserInformation user;
            try
            {
                user = _userHandler.MapUser(result.User);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting votings for user!");
                return Unauthorized();
            }

            var votingsForUser = await _votingStore.GetForUserId(user.UserId);
            var resultForSerialization = votingsForUser.Select(x => new
            {
                Title = x.Title,
                Description = x.Description ?? string.Empty,
                Id = x.Id
            });
            return new JsonResult(resultForSerialization.ToArray());
        }

        public async Task<IActionResult> GetVotingDetails(string id)
        {
            if (await _CheckAuthorization(id, Operation.Read) == false)
                return Unauthorized();

            var voting = await _votingStore.GetById(_GetId(id));
            var result = new
            {
                Id = voting.Id.ToString(CultureInfo.InvariantCulture),
                Title = voting.Title,
                Description = voting.Description,
                Questions = voting.Questions.Select(x =>
                    new
                    {
                        Text = x.QuestionText,
                        AnswerOptions = x.AnswerOptions.Select(a => a.AnswerText).ToArray(),
                        Id = x.Id.ToString(CultureInfo.InvariantCulture)
                    }).ToArray()
            };

            return new JsonResult(result);
        }

        public async Task<IActionResult> GetQuestion(string votingId, string questionId)
        {
            if (await _CheckAuthorization(votingId, Operation.UpdateQuestion) == false)
                return Unauthorized();

            var voting = await _votingStore.GetById(_GetId(votingId));
            var qid = _GetId(questionId);
            var question = voting.Questions.SingleOrDefault(x => x.Id == qid);
            if (question == null)
                return BadRequest("Invalid question id"); // TODO
            var result = new
            {
                Id = question.Id,
                Text = question.QuestionText,
                Description = _GetDescription(question),
                AnswerOptions = question.AnswerOptions.Select(x =>
                    new
                    {
                        Id = x.Id,
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

        private async Task<bool> _CheckAuthorization(string id, Operation operation)
        {
            _user = null;
            var auth = _authentication.CheckToken(Request.Headers["Authorization"]);
            if (!auth.Success)
            {
                return false;
            }

            try
            {
                _user = _userHandler.MapUser(auth.User);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting votings for user!");
                return false;
            }

            long? idVal = string.IsNullOrEmpty(id) ? (long?)null : long.Parse(id);
            var authorized = await _authManager.IsAuthorized(_user.UserId, idVal, operation);
            if (!authorized)
            {
                _logger.LogWarning("User tried to open voting without being authorized");
                return false;
            }

            return true;
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
            return View(new EditVotingModel {VotingId = id });
        }

        [HttpGet]
        public async Task<IActionResult> EditQuestion(string id, string qid)
        {
            return View(new EditVotingQuestionModel { VotingId = id, QuestionId = qid});
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVoting(string id, string title, string desc)
        {
            var idVal = _GetId(id);
            var operation = idVal == 0 ? Operation.Create : Operation.UpdateVoting;
            if (await _CheckAuthorization(id, operation) == false)
                return Unauthorized();

            if (idVal != 0)
            {
                return await _UpdateVoting(id, title, desc);
            }

            return await _InsertVoting(title, desc, _user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVotingQuestion(string id, string qid, string title, string desc)
        {
            if (await _CheckAuthorization(id, Operation.UpdateQuestion) == false)
                return Unauthorized();

            var voting = await _votingStore.GetById(_GetId(id)); // TODO - handle missing voting
            var question = _GetQuestion(title, desc);
            question.Id = _GetId(qid);
            if (question.Id == 0)
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
            if (await _CheckAuthorization(id, Operation.UpdateQuestion) == false)
                return Unauthorized();

            await _votingStore.DeleteQuestion(_GetId(id), _GetId(qid));
            return Ok(); // TODO err handling
        }

        private static Question _GetQuestion(string title, string desc)
        {
            var details = string.IsNullOrEmpty(desc)
                ? new List<QuestionDetail>() 
                : new List<QuestionDetail>
                {
                    new QuestionDetail()
                    {
                        DetailType = QuestionDetailType.AdditionalInfo,
                        DetailValue = desc
                    }
                };

            var question = new Question
            {
                AnswerOptions = new List<AnswerOption>(),
                QuestionText = title ?? "***---",
                Status = QuestionStatus.InPreparation,
                Details = details.ToList()
            };
            return question;
        }

        private async Task<IActionResult> _InsertVoting(string title, string desc, UserInformation user)
        {
            if (await _CheckAuthorization(null, Operation.Create) == false)
                return Unauthorized();
            
            StandardVoting voting = new StandardVoting()
            {
                Creator = user.UserId,
                DateCreated = DateTime.UtcNow,
                Description = desc,
                Questions = new List<Question>(),
                Title = title,
                Visibility = VotingVisibility.OwnerOnly
            };

            await _votingStore.Insert(voting).ConfigureAwait(false);

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