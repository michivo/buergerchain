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
            var startIndex = imageData.IndexOf(':');
            var endIndex = imageData.IndexOf(';');
            if (startIndex == -1 || endIndex == -1 || endIndex < startIndex)
                throw new ArgumentException("ImageData is invalid" + _GetImageDataForException(imageData));
            return imageData.Substring(startIndex + 1, endIndex - startIndex - 1);
        }

        public static byte[] GetImageData(this string imageData)
        {
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
    }
}
