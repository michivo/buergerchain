namespace FreieWahl.Security.Signing.Common
{
    public interface ISignatureProvider
    {
        byte[] SignData(byte[] data);

        bool IsSignatureValid(byte[] data, byte[] signature);
    }
}
