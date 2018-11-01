using System;

namespace FreieWahl.Voting.Registrations
{
    public class OpenRegistration
    {
        public string VotingId { get; set; }

        public string VoterName { get; set; }

        public string VoterIdentity { get; set; }

        public DateTime RegistrationTime { get; set; }

        public string Id { get; set; }
    }
}
