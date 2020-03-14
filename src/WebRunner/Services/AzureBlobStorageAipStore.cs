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

        private async Task EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new Exception("The AzureBlobStorageAipStore needs to be initialized before it can be used");
            }
            await this._container.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        public async Task<bool> Exists(string id)
        {
            await EnsureInitialized().ConfigureAwait(false);
            return await _container.GetBlobReference(id).ExistsAsync().ConfigureAwait(false);
        }

        public async Task<Aip> Get(string id)
        {
            await EnsureInitialized().ConfigureAwait(false);
            var blobReference = _container.GetBlockBlobReference(id);
            using (var streamReader = new StreamReader(await blobReference.OpenReadAsync().ConfigureAwait(false)))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    return _serializer.Deserialize<Aip>(jsonReader);
                }
            }
        }

        public async Task<string[]> GetAllStoredIds()
        {
            await EnsureInitialized().ConfigureAwait(false);
            var blobs = new List<IListBlobItem>();
            BlobResultSegment results = null;
            do
            {
                results = await _container.ListBlobsSegmentedAsync(
                    prefix: null,
                    useFlatBlobListing: true,
                    blobListingDetails: BlobListingDetails.None,
                    maxResults: 1000,
                    currentToken: results?.ContinuationToken,
                    options: null,
                    operationContext: null
                ).ConfigureAwait(false);
                blobs.AddRange(results.Results);
            } while (results != null && results.ContinuationToken != null);
            return blobs.OfType<CloudBlockBlob>().Select(b => b.Name).ToArray();
        }

        public async Task<string> Store(Aip aip)
        {
            await EnsureInitialized().ConfigureAwait(false);
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