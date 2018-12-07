﻿using System.Threading.Tasks;

namespace FreieWahl.Application.Tracking
{
    public interface ITracker
    {
        Task Track(string path, string clientAddress, string userAgent);

        Task TrackSpending(string votingId, string cost);
    }
}