using FreieWahl.Models.VotingAdministration;

namespace FreieWahl.Models.Voting
{
    public class VotingQuestionModel
    {
        public QuestionModel Question { get; set; }

        public VotingQuestionStatus AnswerStatus { get; set; }

        public string[] SelectedAnswerIds { get; set; }
    }
}
