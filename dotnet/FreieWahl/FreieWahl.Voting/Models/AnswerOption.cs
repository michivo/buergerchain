namespace FreieWahl.Voting.Models
{
    public class AnswerOption
    {
        public string Id { get; set; }

        public string AnswerText { get; set; }

        public AnswerDetail[] Details { get; set; }
    }
}
