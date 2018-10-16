using System;
using System.Collections.Generic;

namespace FreieWahl.Models.VotingAdministration
{
    public class EditVotingModel
    {
        public string VotingId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageData { get; set; }
        public List<QuestionModel> Questions { get; set; }
        public string UserInitials { get; set; }
        public int StartDate { get; set; }
        public int EndDate { get; set; }
        public string RegistrationUrl { get; set; }
    }
}
