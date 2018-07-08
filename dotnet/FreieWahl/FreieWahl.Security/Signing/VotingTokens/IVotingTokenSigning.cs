namespace FreieWahl.Security.Signing.VotingTokens
{
    public interface IVotingTokenSigning
    {
        string Sign(string token);
    }
}
