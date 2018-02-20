namespace FreieWahl.Security.TimeStamps
{
    /// <summary>
    /// PKI statuses according to RFC 3161
    /// </summary>
    internal enum PkiStatus
    {
        /// <summary>
        /// When the PKIStatus contains the value zero a TimeStampToken, as requested, is present.
        /// </summary>
        Granted = 0,

        /// <summary>
        /// When the PKIStatus contains the value one a TimeStampToken, with modifications, is present.
        /// </summary>
        GrantedWithMods = 1,

        /// <summary>
        /// When the PKIStatus contains the value two a TimeStamp request was rejected.
        /// </summary>
        Rejection = 2,

        /// <summary>
        /// The request body part has not yet been processed, expect to hear more later.
        /// </summary>
        Waiting = 3,

        /// <summary>
        /// A warning that a revocation is imminent.
        /// </summary>
        RevocationWarning = 4,

        /// <summary>
        /// Revocation has occurred.
        /// </summary>
        RevocationNotification = 5
    }
}
