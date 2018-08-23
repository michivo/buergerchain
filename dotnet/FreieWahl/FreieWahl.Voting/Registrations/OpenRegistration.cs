using System;

namespace FreieWahl.Voting.Registrations
{
    public class OpenRegistration
    {
        public long RegistrationId { get; set; }

        public long VotingId { get; set; }

        public string VoterName { get; set; }

        public string VoterIdentity { get; set; }

        public DateTime RegistrationTime { get; set; }

        public string EMailAdress { get; set; }

        public string RegistrationStoreId { get; set; }
    }
}
