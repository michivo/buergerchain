using System.Collections.Generic;

namespace FreieWahl.Voting.Models
{
    public class Vote
    {
        public long VotingId { get; set; }

        public int QuestionIndex { get; set; }

        public List<string> SelectedAnswerIds { get; set; }

        public string Token { get; set; }

        public string SignedToken { get; set; }

        public string TimestampData { get; set; }

        public string PreviousBlockSignature { get; set; }
    }
}
