using System;
using FreieWahl.Voting.Common;

namespace FreieWahl.Voting.Models
{
    public class AnswerDetail : IEquatable<AnswerDetail>
    {
        public AnswerDetailType DetailType { get; set; }

        public string DetailValue { get; set; }

        public bool Equals(AnswerDetail other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return DetailType == other.DetailType &&
                   DetailValue.EqualsDefault(other.DetailValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;

            return Equals((AnswerDetail) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) DetailType * 397) ^ (DetailValue != null ? DetailValue.GetHashCode() : 0);
            }
        }
    }
}
