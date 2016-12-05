using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoF.StagingStore.Azure
{
    /// <remarks>This class is not thread safe</remarks>
    public class CachingStagingStore : IStagingStore
    {
        private IStagingStore _stagingStore;
        private Dictionary<string, IComponentStagingStore> _cache = new Dictionary<string, IComponentStagingStore>();

        public CachingStagingStore(IStagingStore stagingStore)
        {
            this._stagingStore = stagingStore;
        }

        public async Task<IComponentStagingStore> GetComponentStagingStoreAsync(string componentIdentifier)
        {
            if (!_cache.ContainsKey(componentIdentifier))
            {
                var stagingStore = new CachingComponentStagingStore(await _stagingStore.GetComponentStagingStoreAsync(componentIdentifier).ConfigureAwait(false));
                await stagingStore.InitializeCache().ConfigureAwait(false);
                _cache[componentIdentifier] = stagingStore;
            }
            return _cache[componentIdentifier];
        }
    }
}