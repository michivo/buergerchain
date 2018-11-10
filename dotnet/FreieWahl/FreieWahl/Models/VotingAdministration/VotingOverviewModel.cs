using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreieWahl.Models.VotingAdministration
{
    public class VotingOverviewModel : PageModel
    {
        public string Image { get; set; }
        public string FullName { get; set; }
        public string Initials { get; set; }
    }
}
