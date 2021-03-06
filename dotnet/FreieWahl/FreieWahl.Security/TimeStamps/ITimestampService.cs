﻿using System.Threading.Tasks;
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
        /// <param name="checkCertificate">flag whether the certficate used to create the time stamp should be verified, too</param>
        /// <returns>a time stamp token issued by a time stamp authority</returns>
        Task<TimeStampToken> GetToken(byte[] data, bool checkCertificate = true);

        /// <summary>
        /// only used for testing purposes - checks if a timestamp is valid
        /// </summary>
        /// <param name="token">a timestamp token for some data</param>
        /// <param name="data">some data</param>
        /// <returns>checks if the timestamp is actually valid</returns>
        bool CheckTokenContent(TimeStampToken token, byte[] data);
    }
}