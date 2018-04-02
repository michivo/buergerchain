using System.Collections.Generic;
using FreieWahl.Voting.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace FreieWahl.Models.VotingAdministration
{
    public class VotingOverviewModel : PageModel
    {
        public List<StandardVoting> Votings { get; set; }
        public LocalizedString Title { get; set; }
        public LocalizedString Header { get; set; }
    }
}
