using System;
using FreieWahl.Models.VotingAdministration;

namespace FreieWahl.Models.Voting
{
    public class VoteModel
    {
        public QuestionModel[] Questions { get; set; }

        public string VotingTitle { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
