using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FreieWahl.Models.VotingAdministration;
using FreieWahl.Security.Authentication;
using FreieWahl.Security.UserHandling;
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

        public VotingAdministrationController(ILogger<HomeController> logger,
            IVotingStore votingStore,
            IStringLocalizer<VotingAdministrationController> localizer,
            IJwtAuthentication authentication,
            IUserHandler userHandler)
        {
            _logger = logger;
            _votingStore = votingStore;
            _localizer = localizer;
            _authentication = authentication;
            _userHandler = userHandler;
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

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var result = _authentication.CheckToken(Request.Headers["Authorization"]);
            if (!result.Success)
                return Unauthorized();
            var voting = await _votingStore.GetById(id);
            var model = new EditVotingModel {Header = "fara", Title = "foro", Voting = voting};
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(long id, EditVotingModel inputs)
        {
            var result = _authentication.CheckToken(Request.Headers["Authorization"]);
            if (!result.Success)
                return Unauthorized();
            var user = _userHandler.MapUser(result.User);
            if(!user.UserId.Equals(inputs.Voting.Creator, StringComparison.InvariantCulture))
                return Unauthorized();

            await _votingStore.Update(inputs.Voting);
            return Ok();
        }
    }
}