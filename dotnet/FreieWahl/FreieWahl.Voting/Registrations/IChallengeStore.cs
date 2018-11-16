using System.Threading.Tasks;

namespace FreieWahl.Voting.Registrations
{
    public interface IChallengeStore
    {
        Task SetChallenge(Challenge challenge);

        Task<Challenge> GetChallenge(string registrationId);

        Task DeleteChallenge(string registrationId);

        Task DeleteChallenges(string votingId);
    }
}
