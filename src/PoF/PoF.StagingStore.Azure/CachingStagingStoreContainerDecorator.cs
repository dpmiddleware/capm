using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.StagingStore.Azure
{
    /// <remarks>This class is not thread safe</remarks>
    public class CachingStagingStoreContainerDecorator : IStagingStoreContainer
    {
        private IStagingStoreContainer _stagingStoreContainer;
        private Dictionary<Guid, IStagingStore> _cachedStores;

        public CachingStagingStoreContainerDecorator(IStagingStoreContainer stagingStoreContainer)
        {
            this._stagingStoreContainer = stagingStoreContainer;
        }
        
        public async Task PopulateCache()
        {
            if (_cachedStores != null)
            {
                throw new Exception("Not allowed to call the populate cache methods more than once");
            }
            var cachedStores = new Dictionary<Guid, IStagingStore>();
            var contextIdsStored = await _stagingStoreContainer.GetStoredContextIds().ConfigureAwait(false);
            foreach (var contextId in contextIdsStored)
            {
                cachedStores.Add(contextId, new CachingStagingStore(await _stagingStoreContainer.GetStoreForContextIdAsync(contextId).ConfigureAwait(false)));
            }
            _cachedStores = cachedStores;
        }

        private void EnsureCacheIsPopulated()
        {
            if (_cachedStores == null)
            {
                throw new Exception("The PopulateCache method on the CachingstagingStoreContainerDecorator must be called before calling other methods on it.");
            }
        }

        public async Task CreateStoreForContextIdAsync(Guid contextId)
        {
            EnsureCacheIsPopulated();
            await _stagingStoreContainer.CreateStoreForContextIdAsync(contextId).ConfigureAwait(false);
            _cachedStores.Add(contextId, new CachingStagingStore(await _stagingStoreContainer.GetStoreForContextIdAsync(contextId).ConfigureAwait(false)));
        }

        public Task<Guid[]> GetStoredContextIds()
        {
            EnsureCacheIsPopulated();
            return Task.FromResult(_cachedStores.Keys.ToArray());
        }

        public async Task<IStagingStore> GetStoreForContextIdAsync(Guid contextId)
        {
            EnsureCacheIsPopulated();
            if (_cachedStores.ContainsKey(contextId))
            {
                return _cachedStores[contextId];
            }
            else
            {
                var contextIdsStored = await _stagingStoreContainer.GetStoredContextIds().ConfigureAwait(false);
                if (contextIdsStored.Contains(contextId))
                {
                    _cachedStores.Add(contextId, new CachingStagingStore(await _stagingStoreContainer.GetStoreForContextIdAsync(contextId).ConfigureAwait(false)));
                    return _cachedStores[contextId];
                }
                else
                {
                    throw new Exception($"For some reason the cache has not been populated with any staging store for the id '{contextId}'. This is an error condition, meaning the cache is improperly written or the request for this store is erraneous.");
                }
            }
        }

        public async Task RemoveStoreForContextIdAsync(Guid contextId)
        {
            EnsureCacheIsPopulated();
            if (_cachedStores.ContainsKey(contextId))
            {
                await _stagingStoreContainer.RemoveStoreForContextIdAsync(contextId).ConfigureAwait(false);
                _cachedStores.Remove(contextId);
            }
            else
            {
                throw new Exception($"For some reason the cache has not been populated with any staging store for the id '{contextId}', so it cannot be deleted. This is an error condition, meaning the cache is improperly written or the request for this store is erraneous.");
            }
        }
    }
}
