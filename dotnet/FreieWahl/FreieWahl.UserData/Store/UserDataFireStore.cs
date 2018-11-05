using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FreieWahl.Common;
using Google.Cloud.Datastore.V1;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Google.Protobuf;

namespace FreieWahl.UserData.Store
{
    public class UserDataFireStore : IUserDataStore
    {
        private readonly StorageClient _storage;
        private readonly FirestoreDb _db;

        private readonly string _collection = "UserData";
        public static readonly string TestNamespace = "test";
        public static readonly string DevNamespace = "dev";
        private readonly string _bucketName;

        public UserDataFireStore(string projectId, string bucketName, string prefix = "")
        {
            _storage = StorageClient.Create();
            _collection = prefix + _collection;
            _bucketName = bucketName;
            _db = FirestoreDb.Create(projectId);
        }
        
        public async Task SaveUserImage(string userId, string imageData)
        {
            var mimeType = imageData.GetMimeType();
            var rawData = imageData.GetImageData();

            var imageAcl = PredefinedObjectAcl.PublicRead;

            var imageObject = await _storage.UploadObjectAsync(
                _bucketName,
                userId,
                mimeType,
                new MemoryStream(rawData),
                new UploadObjectOptions { PredefinedAcl = imageAcl }
            );

            var url = imageObject.MediaLink;

            await _db.Collection(_collection).Document(userId).SetAsync(new Dictionary<string, object>
            {
                { "UserImageUrl", url }
            });
        }

        public async Task<string> GetUserImage(string userId)
        {
            var doc = await _db.Collection(_collection).Document(userId).GetSnapshotAsync();
            return !doc.Exists ? null : doc.GetValue<string>("UserImageUrl");
        }
    }
}

