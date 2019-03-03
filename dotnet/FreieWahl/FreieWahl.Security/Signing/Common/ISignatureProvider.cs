namespace FreieWahl.Security.Signing.Common
{
    /// <summary>
    /// signature provider, supports signing data - in the default implementation, SHA256withRSA is used for signing
    /// </summary>
    public interface ISignatureProvider
    {
        /// <summary>
        /// signs some data (typically using SHA256withRSA or something similar)
        /// </summary>
        /// <param name="data">some data</param>
        /// <returns>signature for this data</returns>
        byte[] SignData(byte[] data);

        /// <summary>
        /// only used in test code - checks if a signature is valid
        /// </summary>
        /// <param name="data">some data</param>
        /// <param name="signature">the signature for this data</param>
        /// <returns>true, if the signature is a valid signature for this data</returns>
        bool IsSignatureValid(byte[] data, byte[] signature);
    }
}
