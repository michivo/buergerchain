﻿using System;
using FreieWahl.Voting.Common;

namespace FreieWahl.Voting.Models
{
    public class AnswerDetail : IEquatable<AnswerDetail>
    {
        public AnswerDetailType DetailType { get; set; }

        public string DetailValue { get; set; }

        public bool Equals(AnswerDetail other)
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

            return Equals((AnswerDetail) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) DetailType * 397) ^ (DetailValue != null ? DetailValue.ToLowerInvariant().GetHashCode() : 0);
            }
        }
    }
}
