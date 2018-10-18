using FreieWahl.Models.VotingAdministration;

namespace FreieWahl.Models.Voting
{
    public class AnsweredQuestion
    {
        public QuestionModel Question { get; set; }

        public string[] SelectedAnswerIds { get; set; }

        public bool WasAnswered { get; set; }
    }
}
