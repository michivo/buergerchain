using System;
using System.Linq;
using FreieWahl.Voting.Common;

namespace FreieWahl.Voting.Models
{
    public class Question : IEquatable<Question>
    {
        private QuestionDetail[] _details;
        private AnswerOption[] _answerOptions;

        public Question()
        {
            Details = new QuestionDetail[0];
            AnswerOptions = new AnswerOption[0];
        }

        public string QuestionText { get; set; }

        public QuestionDetail[] Details
        {
            get => _details;
            set
            {
                if (value == null)
                    return;
                _details = value;
            }
        }

        public AnswerOption[] AnswerOptions
        {
            get => _answerOptions;
            set
            {
                if (value == null)
                    return;

                _answerOptions = value;
            }
        }

        public bool Equals(Question other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return QuestionText.EqualsDefault(other.QuestionText) && 
                   Details.SequenceEqual(other.Details) && 
                   AnswerOptions.SequenceEqual(other.AnswerOptions);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Question) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (QuestionText != null ? QuestionText.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Details != null ? Details.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AnswerOptions != null ? AnswerOptions.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
