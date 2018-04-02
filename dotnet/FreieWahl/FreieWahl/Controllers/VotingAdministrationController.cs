using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Models;
using FreieWahl.Models.VotingAdministration;
using FreieWahl.Voting.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FreieWahl.Controllers
{
    public class VotingAdministrationController : Controller
    {
        private readonly IVotingStore _votingStore;
        private readonly IStringLocalizer<VotingAdministrationController> _localizer;

        public VotingAdministrationController(IVotingStore votingStore,
            IStringLocalizer<VotingAdministrationController> localizer)
        {
            _votingStore = votingStore;
            _localizer = localizer;
        }

        public async Task<IActionResult> Overview()
        {
            var model = new VotingOverviewModel();
            var votings = await _votingStore.GetAll();
            model.Votings = votings.ToList();
            model.Title = _localizer["Title"];
            model.Header = _localizer["Header"];
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var votings = await _votingStore.GetAll();
            var voting = votings.FirstOrDefault(x => x.Id.Equals(id));
            var model = new EditVotingModel {Header = "fara", Title = "foro", Voting = voting};
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(long id, EditVotingModel inputs)
        {
            return Ok();
        }
    }
}