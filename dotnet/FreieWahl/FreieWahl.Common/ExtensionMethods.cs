using System;

namespace FreieWahl.Common
{
    public static class ExtensionMethods
    {
        public static long? ToId(this string idString)
        {
            return long.TryParse(idString, out var result) ? result : (long?)null;
        }

        public static string GetMimeType(this string imageData)
        {
            if (string.IsNullOrEmpty(imageData))
                return string.Empty;

            var startIndex = imageData.IndexOf(':');
            var endIndex = imageData.IndexOf(';');
            if (startIndex == -1 || endIndex == -1 || endIndex < startIndex)
                throw new ArgumentException("ImageData is invalid" + _GetImageDataForException(imageData));
            return imageData.Substring(startIndex + 1, endIndex - startIndex - 1);
        }

        public static byte[] GetImageData(this string imageData)
        {
            if (string.IsNullOrEmpty(imageData))
                return new byte[0];

            var startIndex = imageData.IndexOf(';') + 8;
            if (startIndex >= imageData.Length)
                throw new ArgumentException("ImageData is invalid, data part is invalid" + _GetImageDataForException(imageData));
            return Convert.FromBase64String(imageData.Substring(startIndex));
        }

        private static string _GetImageDataForException(string imageData)
        {
            if (string.IsNullOrEmpty(imageData))
                return "---";
            if (imageData.Length < 50)
                return imageData;
            return imageData.Substring(0, 50);
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int ToSecondsSinceEpoch(this DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Only UTC timestamps are supported");

            return (int)dateTime.Subtract(Epoch).TotalSeconds;
        }

        public static int ToMillisecondsSinceEpoch(this DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Only UTC timestamps are supported");

            return (int)dateTime.Subtract(Epoch).TotalSeconds;
        }
    }
}
