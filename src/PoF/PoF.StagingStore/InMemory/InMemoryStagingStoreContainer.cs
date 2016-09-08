using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.StagingStore.InMemory
{
    public class InMemoryStagingStoreContainer : IStagingStoreContainer
    {
        private Dictionary<Guid, IStagingStore> _stagingStoreDictionary = new Dictionary<Guid, IStagingStore>();

        public Task CreateStoreForContextIdAsync(Guid contextId)
        {
            _stagingStoreDictionary.Add(contextId, new InMemoryStagingStore());
            return Task.FromResult(true);
        }

        public Task<IStagingStore> GetStoreForContextIdAsync(Guid contextId)
        {
            return Task.FromResult(_stagingStoreDictionary[contextId]);
        }

        public Task RemoveStoreForContextIdAsync(Guid contextId)
        {
            _stagingStoreDictionary.Remove(contextId);
            return Task.FromResult(true);
        }
    }
}
