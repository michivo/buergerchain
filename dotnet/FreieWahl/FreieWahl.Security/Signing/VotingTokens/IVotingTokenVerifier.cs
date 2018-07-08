namespace FreieWahl.Security.Signing.VotingTokens
{
    public interface IVotingTokenVerifier
    {
        bool Verify(string signature, string origMessage);
    }
}
