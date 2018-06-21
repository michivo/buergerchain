using System;

namespace FreieWahl.Voting.Common
{
    internal static class IdHelper
    {
        private static Random _rand;

        static IdHelper()
        {
            _rand = new Random();
        }

        public static long GetId()
        {
            byte[] buf = new byte[8];
            _rand.NextBytes(buf);
            return Math.Abs(BitConverter.ToInt64(buf, 0));
        }
    }
}
