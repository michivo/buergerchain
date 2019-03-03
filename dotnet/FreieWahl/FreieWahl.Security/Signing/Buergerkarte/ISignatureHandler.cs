namespace FreieWahl.Security.Signing.Buergerkarte
{
    public interface ISignatureHandler
    {
        /// <summary>
        /// gets the data, signee and certificate that was used to create the given signed data
        /// </summary>
        /// <param name="signedData">CMS signed data</param>
        /// <returns>the decoded data from the CMS message, i.e. the signee id, signee name, signature certificate and the original data</returns>
        SignedData GetSignedContent(string signedData);
    }
}