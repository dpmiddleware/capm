using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PoF.StagingStore.Azure
{
    internal class AzureBlobStorageComponentStagingStore : IComponentStagingStore
    {
        private CloudBlobContainer _cloudBlobContainer;
        private string _componentIdentifier;

        public AzureBlobStorageComponentStagingStore(CloudBlobContainer cloudBlobContainer, string componentIdentifier)
        {
            this._cloudBlobContainer = cloudBlobContainer;
            this._componentIdentifier = componentIdentifier;
        }

        private string Prefix
        {
            get { return $"{_componentIdentifier}/"; }
        }

        private CloudBlockBlob GetBlobReference(string identifier)
        {
            return _cloudBlobContainer.GetBlockBlobReference(Prefix + identifier);
        }

        public Task<string[]> GetAvailableIdentifiersAsync()
        {
            var containerUriLength = _cloudBlobContainer.Uri.ToString().Length;
            var blobs = _cloudBlobContainer.ListBlobs(prefix: Prefix, useFlatBlobListing: true);
            var blobNames = blobs.OfType<CloudBlockBlob>().Select(b => b.Name.Substring(Prefix.Length)).ToArray();
            return Task.FromResult(blobNames);
        }

        public Task<Stream> GetItemAsync(string identifier)
        {
            return GetBlobReference(identifier).OpenReadAsync();
        }

        public Task<bool> HasItemAsync(string identifier)
        {
            return GetBlobReference(identifier).ExistsAsync();
        }

        public Task RemoveItemAsync(string identifier)
        {
            return GetBlobReference(identifier).DeleteAsync();
        }

        public async Task SetItemAsync(string identifier, Stream stream)
        {
            var blob = GetBlobReference(identifier);
            await blob.UploadFromStreamAsync(stream).ConfigureAwait(false);
        }
    }
}