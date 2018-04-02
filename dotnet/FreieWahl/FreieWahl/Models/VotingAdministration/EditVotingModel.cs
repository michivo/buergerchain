using FreieWahl.Voting.Models;

namespace FreieWahl.Models.VotingAdministration
{
    public class EditVotingModel
    {
        public string Title { get; set; }
        public string Header { get; set; }

        public StandardVoting Voting { get; set; }
    }
}
