using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FreieWahl.Application.Authentication;
using FreieWahl.Models.VotingAdministration;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.UserHandling;
using FreieWahl.Voting.Models;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

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

        public async Task<IActionResult> GetVotingDetails(long id)
        {
            if (await _CheckAuthorization(id, Operation.Read) == false)
                return Unauthorized();

            var voting = await _votingStore.GetById(id);
            var result = new
            {
                Id = voting.Id,
                Title = voting.Title,
                Description = voting.Description,
                Questions = voting.Questions.Select(x =>
                    new
                    {
                        Text = x.QuestionText,
                        AnswerOptions = x.AnswerOptions.Select(a => a.AnswerText).ToArray(),
                        Id = x.Id
                    }).ToArray()
            };

            return new JsonResult(result);
        }

        public async Task<IActionResult> GetQuestion(long votingId, long questionId)
        {
            if (await _CheckAuthorization(votingId, Operation.UpdateQuestion) == false)
                return Unauthorized();

            var voting = await _votingStore.GetById(votingId);
            var question = voting.Questions.SingleOrDefault(x => x.Id == questionId);
            if (question == null)
                return BadRequest("Invalid question id"); // TODO
            var result = new
            {
                Id = question.Id,
                Text = question.QuestionText,
                AnswerOptions = question.AnswerOptions.Select(x =>
                    new
                    {
                        Id = x.Id,
                        Text = x.AnswerText
                    })
            };
            return new JsonResult(result);
        }

        private async Task<bool> _CheckAuthorization(long? id, Operation operation)
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

            var authorized = await _authManager.IsAuthorized(_user.UserId, id, operation);
            if (!authorized)
            {
                _logger.LogWarning("User tried to open voting without being authorized");
                return false;
            }

            return true;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            return View(new EditVotingModel {VotingId = id });
        }

        [HttpGet]
        public async Task<IActionResult> EditQuestion(long id, long qid)
        {
            return View(new EditVotingQuestionModel { VotingId = id, QuestionId = qid});
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVoting(long id, string title, string desc)
        {
            var operation = id == 0 ? Operation.Create : Operation.UpdateVoting;
            long? queryId = id == 0 ? (long?)null : id;
            if (await _CheckAuthorization(queryId, operation) == false)
                return Unauthorized();

            if (id != 0)
            {
                return await _UpdateVoting(id, title, desc);
            }

            return await _InsertVoting(title, desc, _user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVotingQuestion(long id, long qid, string title, string desc)
        {
            if (await _CheckAuthorization(id, Operation.UpdateQuestion) == false)
                return Unauthorized();

            var voting = await _votingStore.GetById(id);
            var question = new Question
            {
                AnswerOptions = new AnswerOption[0],
                QuestionText = title,
                Status = QuestionStatus.InPreparation,
                Details = new QuestionDetail[]
                {
                    new QuestionDetail()
                    {
                        DetailType = QuestionDetailType.AdditionalInfo,
                        DetailValue = desc
                    }
                }
            };
            if (qid == 0)
            {
                await _votingStore.AddQuestion(id, question);
                return Ok();
            }

            for (int i = 0; i < voting.Questions.Length; i++)
            {
                if (voting.Questions[i].Id == qid)
                {
                    voting.Questions[i] = question;
                    return Ok();
                }
            }

            return BadRequest(); // TODO
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
                Questions = new Question[0],
                Title = title,
                Visibility = VotingVisibility.OwnerOnly
            };

            await _votingStore.Insert(voting).ConfigureAwait(false);

            return Ok();
        }

        private async Task<IActionResult> _UpdateVoting(long id, string title, string desc)
        {
            var voting = await _votingStore.GetById(id);
            voting.Title = title;
            voting.Description = desc;
            await _votingStore.Update(voting).ConfigureAwait(false);

            return Ok();
        }
    }
}