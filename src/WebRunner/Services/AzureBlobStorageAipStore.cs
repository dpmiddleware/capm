using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebRunner.Models;

namespace WebRunner.Services
{
    public class AzureBlobStorageAipStore : IAipStore
    {
        private CloudBlobClient _cloudBlobClient;
        private string _connectionString;
        private CloudBlobContainer _container;
        private CloudStorageAccount _storageAccount;
        private bool _isInitialized;
        private static readonly JsonSerializer _serializer = new JsonSerializer();

        public AzureBlobStorageAipStore(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public async Task Initialize()
        {
            if (_isInitialized)
            {
                throw new Exception("Can only initialize the AzureBlobStorageAipStore once");
            }
            this._storageAccount = CloudStorageAccount.Parse(_connectionString);
            this._cloudBlobClient = _storageAccount.CreateCloudBlobClient();
            this._container = _cloudBlobClient.GetContainerReference("dps-aip-store");
            await this._container.CreateIfNotExistsAsync().ConfigureAwait(false);
            this._isInitialized = true;
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new Exception("The AzureBlobStorageAipStore needs to be initialized before it can be used");
            }
        }

        public Task<bool> Exists(string id)
        {
            EnsureInitialized();
            return _container.GetBlobReference(id).ExistsAsync();
        }

        public async Task<Aip> Get(string id)
        {
            EnsureInitialized();
            var blobReference = _container.GetBlockBlobReference(id);
            using (var streamReader = new StreamReader(await blobReference.OpenReadAsync().ConfigureAwait(false)))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    return _serializer.Deserialize<Aip>(jsonReader);
                }
            }
        }

        public Task<string[]> GetAllStoredIds()
        {
            EnsureInitialized();
            return Task.FromResult(_container.ListBlobs(useFlatBlobListing: true).OfType<CloudBlockBlob>().Select(b => b.Name).ToArray());
        }

        public async Task<string> Store(Aip aip)
        {
            EnsureInitialized();
            var newId = Guid.NewGuid().ToString();
            while (await Exists(newId))
            {
                newId = Guid.NewGuid().ToString();
            }
            var newBlob = _container.GetBlockBlobReference(newId);
            var stream = new MemoryStream();
            try
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    using (var jsonWriter = new JsonTextWriter(streamWriter))
                    {
                        _serializer.Serialize(jsonWriter, aip);
                    }
                }
                var bytes = stream.GetBuffer();
                await newBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
            }
            finally
            {
                stream.Dispose();
            }
            return newId;
        }
    }
}