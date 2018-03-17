namespace FreieWahl.Voting.Models
{
    public class Question
    {
        public string QuestionText { get; set; }

        public QuestionDetail[] Details { get; set; }

        public AnswerOption[] AnswerOptions { get; set; }
    }
}
