using System;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;

namespace FreieWahl.UserData.Store
{
    public class UserDataStore : IUserDataStore
    {
        private readonly DatastoreDb _db;
        private const string StoreKind = "StandardVoting";
        public static readonly string TestNamespace = "test";
        public static readonly string DevNamespace = "dev";
        private readonly KeyFactory _keyFactory;

        public UserDataStore(string projectId, string namespaceId = "", DatastoreClient client = null)
        {
            _db = DatastoreDb.Create(projectId, namespaceId, client);
            _keyFactory = new KeyFactory(projectId, namespaceId, StoreKind);
        }
        
        public Task SaveUserImage(string userId, string imageData)
        {
            var mimeType = _GetMimeType(imageData);
            var rawData = _GetRawData(imageData);

            var entity = new Entity()
            {
                Key = _keyFactory.CreateKey(imageData),
                ["ImageData"] = rawData,
                ["MimeType"] = mimeType
            };

            return _db.InsertAsync(entity);
        }

        private string _GetMimeType(string imageData)
        {
            var startIndex = imageData.IndexOf(':');
            var endIndex = imageData.IndexOf(';');
            if(startIndex == -1 || endIndex == -1 || endIndex < startIndex)
                throw new ArgumentException("ImageData is invalid" + _GetImageDataForLogging(imageData));
            return imageData.Substring(startIndex + 1, endIndex - startIndex - 1);
        }

        private byte[] _GetRawData(string imageData)
        {
            var startIndex = imageData.IndexOf(';') + 8;
            if (startIndex >= imageData.Length)
                throw new ArgumentException("ImageData is invalid, data part is invalid" + _GetImageDataForLogging(imageData));
            return Convert.FromBase64String(imageData.Substring(startIndex));
        }

        private string _GetImageDataForLogging(string imageData)
        {
            if (string.IsNullOrEmpty(imageData))
                return "---";
            if (imageData.Length < 50)
                return imageData;
            return imageData.Substring(0, 50);
        }

        public async Task<string> GetUserImage(string userId)
        {
            var key = _keyFactory.CreateKey(userId);
            var result = await _db.LookupAsync(key).ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            var rawData = result.Properties["ImageData"].BlobValue;
            var mimeType = result.Properties["MimeType"].StringValue;

            return "data:" + mimeType + ";base64," + rawData.ToBase64();
        }
    }
}

