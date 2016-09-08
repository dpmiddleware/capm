using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoF.StagingStore.InMemory
{
    internal class InMemoryStagingStore : IStagingStore
    {
        private Dictionary<string, IComponentStagingStore> _componentStagingStores = new Dictionary<string, IComponentStagingStore>();
        private object _lockObject = new object();

        public Task<IComponentStagingStore> GetComponentStagingStoreAsync(string componentIdentifier)
        {
            if (!_componentStagingStores.ContainsKey(componentIdentifier))
            {
                lock (_lockObject)
                {
                    if (!_componentStagingStores.ContainsKey(componentIdentifier))
                    {
                        _componentStagingStores[componentIdentifier] = new InMemoryComponentStagingStore();
                    }
                }
            }
            return Task.FromResult(_componentStagingStores[componentIdentifier]);
        }
    }
}