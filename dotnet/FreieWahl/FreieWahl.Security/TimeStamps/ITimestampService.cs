using System.Threading.Tasks;
using Org.BouncyCastle.Tsp;

namespace FreieWahl.Security.TimeStamps
{
    /// <summary>
    /// Service for processing time stamp requests according to RFC 3161.
    /// </summary>
    public interface ITimestampService
    {
        /// <summary>
        /// A hash is calculated for the given data, a time stamp token with a timestamp
        /// from a trusted time stamp authority is returned.
        /// </summary>
        /// <param name="data">the data for which a time stamp should be created</param>
        /// <returns>a time stamp token issued by a time stamp authority</returns>
        Task<TimeStampToken> GetToken(byte[] data);
    }
}