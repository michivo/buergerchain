﻿namespace FreieWahl.Voting.Registrations
{
    public class Registration
    {
        public long RegistrationId { get; set; }

        public long VotingId { get; set; }

        public string VoterName { get; set; }

        public string VoterIdentity { get; set; }
    }
}