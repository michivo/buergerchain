using System.ComponentModel.DataAnnotations;

namespace FreieWahl.Voting.Models
{
    public class AnswerOption
    {
        [Key]
        public long Id { get; set; }

        public string AnswerText { get; set; }

        public AnswerDetail[] Details { get; set; }
    }
}
