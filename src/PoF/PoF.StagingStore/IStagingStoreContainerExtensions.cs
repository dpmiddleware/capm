using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.StagingStore
{
    public static class IStagingStoreContainerExtensions
    {
        public const string SharedStoreComponentIdentifier = "StagingStore.Shared";
        public static async Task<IComponentStagingStore> GetSharedStore(this IStagingStoreContainer stagingStoreContainer, Guid ingestId)
        {
            var ingestStore = await stagingStoreContainer.GetStoreForContextIdAsync(ingestId);
            return await ingestStore.GetComponentStagingStoreAsync(SharedStoreComponentIdentifier);
        }
    }
}
