namespace FreieWahl.Models.Voting
{
    public class QuestionData
    {
        public VotingQuestionModel[] Questions { get; set; }

        public bool DeadlinePassed { get; set; }
    }
}
