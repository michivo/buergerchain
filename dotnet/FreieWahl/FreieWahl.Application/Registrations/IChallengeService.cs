using System.Threading.Tasks;

namespace FreieWahl.Application.Registrations
{
    public interface IChallengeService
    {
        Task SendChallenge(string recipient, string challenge, string votingName);
    }
}
