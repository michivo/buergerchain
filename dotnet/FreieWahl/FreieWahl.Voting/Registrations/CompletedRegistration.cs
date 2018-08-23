using System;

namespace FreieWahl.Voting.Registrations
{
    public class CompletedRegistration
    {
        public string VoterName { get; set; }

        public string VoterIdentity { get; set; }

        public long VotingId { get; set; }

        public string AdminUserId { get; set; }

        public RegistrationDecision Decision { get; set; }

        public DateTime RegistrationTime { get; set; }

        public DateTime DecisionTime { get; set; }
    }
}
