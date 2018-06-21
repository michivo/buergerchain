using System;
using System.Collections.Generic;
using System.Linq;
using FreieWahl.Voting.Common;

namespace FreieWahl.Voting.Models
{
    public class AnswerOption : IEquatable<AnswerOption>
    {
        private List<AnswerDetail> _details;

        public AnswerOption()
        {
            Details = new List<AnswerDetail>();
        }

        public string Id { get; set; }

        public string AnswerText { get; set; }

        public List<AnswerDetail> Details
        {
            get => _details;
            set => _details = value ?? new List<AnswerDetail>();
        }

        public bool Equals(AnswerOption other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (!Id.EqualsDefault(other.Id))
                return false;

            if (!AnswerText.EqualsDefault(other.AnswerText))
                return false;

            return Details.SequenceEqual(other.Details);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AnswerOption)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AnswerText != null ? AnswerText.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Details != null ? Details.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
