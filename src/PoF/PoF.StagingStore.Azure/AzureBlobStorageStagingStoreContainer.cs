using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.StagingStore.Azure
{
    public class AzureBlobStorageStagingStoreContainer : IStagingStoreContainer
    {
        private CloudBlobClient _blobClient;

        public AzureBlobStorageStagingStoreContainer(CloudBlobClient blobClient)
        {
            this._blobClient = blobClient;
        }

        public async Task CreateStoreForContextIdAsync(Guid contextId)
        {
            var container = GetContainerReference(contextId);
            await container.CreateAsync().ConfigureAwait(false);
        }

        private CloudBlobContainer GetContainerReference(Guid contextId)
        {
            return _blobClient.GetContainerReference("ingest-" + contextId.ToString().ToLower());
        }

        public async Task<Guid[]> GetStoredContextIds()
        {
            var containers = new List<CloudBlobContainer>();
            ContainerResultSegment results = null;
            do
            {
                results = await _blobClient.ListContainersSegmentedAsync(
                    prefix: "ingest-",
                    currentToken: results?.ContinuationToken
                ).ConfigureAwait(false);
                containers.AddRange(results.Results);
            } while (results != null && results.ContinuationToken != null);

            var ids = containers.Select(c => Guid.Parse(c.Name.Substring("ingest-".Length))).ToArray();
            return ids;
        }

        public Task<IStagingStore> GetStoreForContextIdAsync(Guid contextId)
        {
            return Task.FromResult<IStagingStore>(new AzureBlobStorageStagingStore(GetContainerReference(contextId)));
        }

        public async Task RemoveStoreForContextIdAsync(Guid contextId)
        {
            var container = GetContainerReference(contextId);
            await container.DeleteAsync();
        }
    }
}
