using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FreieWahl.Voting.Common;
using Microsoft.AspNetCore.Mvc;

namespace FreieWahl.Voting.Models
{
    [Bind("Title", "Creator", "Description", "DateCreated")]
    public class StandardVoting : IEquatable<StandardVoting>
    {
        private List<Question> _questions;

        public StandardVoting()
        {
            Questions = new List<Question>();
        }

        [Key]
        public long Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Creator { get; set; }

        public VotingVisibility Visibility { get; set; }

        public VotingState State { get; set; }

        public int CurrentQuestionIndex { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }

        public List<Question> Questions
        {
            get => _questions;
            set => _questions = value ?? new List<Question>();
        }

        public bool Equals(StandardVoting other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id &&
                   CurrentQuestionIndex == other.CurrentQuestionIndex &&
                   Title.EqualsDefault(other.Title) &&
                   Description.EqualsDefault(other.Description) &&
                   Creator.EqualsDefault(other.Creator) &&
                   Visibility == other.Visibility &&
                   State == other.State &&
                   DateCreated.EqualsDefault(other.DateCreated) &&
                   Questions.SequenceEqual(other.Questions);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StandardVoting)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ CurrentQuestionIndex.GetHashCode();
                hashCode = (hashCode * 397) ^ (Title != null ? Title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Creator != null ? Creator.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Visibility;
                hashCode = (hashCode * 397) ^ (int)State;
                hashCode = (hashCode * 397) ^ DateCreated.GetHashCode();
                hashCode = (hashCode * 397) ^ (Questions != null ? Questions.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
