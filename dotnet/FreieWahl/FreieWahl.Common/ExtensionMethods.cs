namespace FreieWahl.Common
{
    public static class ExtensionMethods
    {
        public static long? ToId(this string idString)
        {
            return long.TryParse(idString, out var result) ? result : (long?)null;
        }
    }
}
