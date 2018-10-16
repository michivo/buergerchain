using System;
using FreieWahl.Models.VotingAdministration;

namespace FreieWahl.Models.Voting
{
    public class VoteModel
    {
        public QuestionModel[] Questions { get; set; }

        public string VotingTitle { get; set; }

        public string VotingDescription { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string ImageData { get; set; }

        public string GetTokensUrl { get; set; }

        public string VotingId { get; set; }

        public string VoterId { get; set; }
    }
}
