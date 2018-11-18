using System;
using FreieWahl.Voting.Common;

namespace FreieWahl.Voting.Registrations
{
    public class OpenRegistration
    {
        public string VotingId { get; set; }

        public string VoterName { get; set; }

        public string VoterIdentity { get; set; }

        public DateTime RegistrationTime { get; set; }

        public string Id { get; set; }

        public RegistrationType RegistrationType { get; set; }
    }
}
