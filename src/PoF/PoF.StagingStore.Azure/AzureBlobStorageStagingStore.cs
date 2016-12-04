using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PoF.StagingStore.Azure
{
    public class AzureBlobStorageStagingStore : IStagingStore
    {
        private CloudBlobContainer _cloudBlobContainer;

        public AzureBlobStorageStagingStore(CloudBlobContainer cloudBlobContainer)
        {
            this._cloudBlobContainer = cloudBlobContainer;
        }

        public Task<IComponentStagingStore> GetComponentStagingStoreAsync(string componentIdentifier)
        {
            return Task.FromResult<IComponentStagingStore>(new AzureBlobStorageComponentStagingStore(_cloudBlobContainer, componentIdentifier));
        }
    }
}
