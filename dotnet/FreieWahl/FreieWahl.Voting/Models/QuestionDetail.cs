using System;
using FreieWahl.Voting.Common;
using Google.Cloud.Firestore;

namespace FreieWahl.Voting.Models
{
    [FirestoreData]
    public class QuestionDetail : IEquatable<QuestionDetail>
    {
        [FirestoreProperty]
        public QuestionDetailType DetailType { get; set; }

        [FirestoreProperty]
        public string DetailValue { get; set; }

        public bool Equals(QuestionDetail other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return DetailType == other.DetailType && 
                   DetailValue.EqualsDefault(other.DetailValue);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;

            return Equals((QuestionDetail) obj);
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
