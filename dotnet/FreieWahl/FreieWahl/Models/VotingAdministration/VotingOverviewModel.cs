using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace FreieWahl.Models.VotingAdministration
{
    public class VotingOverviewModel : PageModel
    {
        public LocalizedString Title { get; set; }
        public LocalizedString Header { get; set; }
    }
}
