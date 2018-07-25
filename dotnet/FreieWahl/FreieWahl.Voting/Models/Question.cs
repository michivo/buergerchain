using System;
using System.Collections.Generic;
using System.Linq;
using FreieWahl.Voting.Common;

namespace FreieWahl.Voting.Models
{
    public class Question : IEquatable<Question>
    {
        private List<QuestionDetail> _details;
        private List<AnswerOption> _answerOptions;

        public Question()
        {
            Details = new List<QuestionDetail>();
            AnswerOptions = new List<AnswerOption>();
        }

        public string QuestionText { get; set; }

        public List<QuestionDetail> Details
        {
            get => _details;
            set => _details = value ?? new List<QuestionDetail>();
        }

        public List<AnswerOption> AnswerOptions
        {
            get => _answerOptions;
            set => _answerOptions = value ?? new List<AnswerOption>();
        }

        public QuestionStatus Status { get; set; }

        public int QuestionIndex { get; set; }

        public bool Equals(Question other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Status.Equals(other.Status) &&
                   QuestionIndex.Equals(other.QuestionIndex) &&
                   QuestionText.EqualsDefault(other.QuestionText) && 
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
                var hashCode = QuestionIndex.GetHashCode();
                hashCode = (hashCode * 397) ^ Status.GetHashCode();
                hashCode = (hashCode * 397) ^ (QuestionText != null ? QuestionText.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Details != null ? Details.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AnswerOptions != null ? AnswerOptions.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
