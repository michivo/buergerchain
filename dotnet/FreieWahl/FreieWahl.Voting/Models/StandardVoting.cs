using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FreieWahl.Voting.Models
{
    [Bind("Title", "Creator", "Description", "DateCreated")]
    public class StandardVoting
    {
        [Key]
        public long Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Creator { get; set; }

        public VotingVisibility Visibility { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }


    }
}
