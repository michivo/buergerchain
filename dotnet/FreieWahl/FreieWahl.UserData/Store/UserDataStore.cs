using System.Threading.Tasks;
using FreieWahl.Common;
using Google.Cloud.Datastore.V1;

namespace FreieWahl.UserData.Store
{
    public class UserDataStore : IUserDataStore
    {
        private readonly DatastoreDb _db;
        private const string StoreKind = "UserData";
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
            var mimeType = imageData.GetMimeType();
            var rawData = imageData.GetImageData();

            var entity = new Entity()
            {
                Key = _keyFactory.CreateKey(userId),
                ["ImageData"] = rawData,
                ["MimeType"] = mimeType
            };
            entity["ImageData"].ExcludeFromIndexes = true;

            return _db.InsertAsync(entity);
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

