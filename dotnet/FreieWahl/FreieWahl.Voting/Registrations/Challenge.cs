namespace FreieWahl.Voting.Registrations
{
    public class Challenge
    {
        public string VotingId { get; set; }

        public string RegistrationId { get; set; }

        public string Value { get; set; }

        public ChallengeType Type { get; set; }
    }
}
