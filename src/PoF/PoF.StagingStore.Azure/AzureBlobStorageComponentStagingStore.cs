using System;
using System.Collections.Generic;
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

        public async Task<string[]> GetAvailableIdentifiersAsync()
        {
            var containerUriLength = _cloudBlobContainer.Uri.ToString().Length;
            var blobs = new List<IListBlobItem>();
            BlobResultSegment results = null;
            do
            {
                results = await _cloudBlobContainer.ListBlobsSegmentedAsync(
                    prefix: Prefix,
                    useFlatBlobListing: true,
                    blobListingDetails: BlobListingDetails.None,
                    maxResults: 1000,
                    currentToken: results?.ContinuationToken,
                    options: null,
                    operationContext: null
                ).ConfigureAwait(false);
                blobs.AddRange(results.Results);
            } while (results != null && results.ContinuationToken != null);
            var blobNames = blobs.OfType<CloudBlockBlob>().Select(b => b.Name.Substring(Prefix.Length)).ToArray();
            return blobNames;
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