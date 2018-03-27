using System;

namespace FreieWahl.Voting.Common
{
    internal static class ExtensionMethods
    {
        public static bool EqualsDefault(this string strA, string strB)
        {
            return String.Equals(strA, strB, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqualsDefault(this DateTime dateA, DateTime dateB)
        {
            return Math.Abs(dateA.Ticks - dateB.Ticks) < 100L;
        }
    }
}
