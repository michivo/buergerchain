namespace FreieWahl.Security.Signing.Buergerkarte
{
    public interface ISignatureHandler
    {
        SignedData GetSignedContent(string signedData);
    }
}