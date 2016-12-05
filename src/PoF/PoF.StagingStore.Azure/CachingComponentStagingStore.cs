using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PoF.StagingStore.Azure
{
    internal class CachingComponentStagingStore : IComponentStagingStore
    {
        private IComponentStagingStore _componentStagingStore;
        private Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();

        public CachingComponentStagingStore(IComponentStagingStore componentStagingStore)
        {
            this._componentStagingStore = componentStagingStore;
        }

        public async Task InitializeCache()
        {
            var storedItems = await _componentStagingStore.GetAvailableIdentifiersAsync().ConfigureAwait(false);
            foreach(var itemId in storedItems)
            {
                using (var itemStream = await _componentStagingStore.GetItemAsync(itemId).ConfigureAwait(false))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await itemStream.CopyToAsync(memoryStream).ConfigureAwait(false);
                        _cache.Add(itemId, memoryStream.GetBuffer());
                    }
                }
            }
        }

        public Task<string[]> GetAvailableIdentifiersAsync()
        {
            return Task.FromResult(_cache.Keys.ToArray());
        }

        public Task<Stream> GetItemAsync(string identifier)
        {
            if (_cache.ContainsKey(identifier))
            {
                return Task.FromResult<Stream>(new MemoryStream(_cache[identifier]));
            }
            else
            {
                throw new KeyNotFoundException($"For some reason the cache has not been populated with item with the ID '{identifier}'. This is an error condition, meaning the cache is improperly written or the request for this item is erraneous.");
            }
        }

        public Task<bool> HasItemAsync(string identifier)
        {
            return Task.FromResult(_cache.ContainsKey(identifier));
        }

        public async Task RemoveItemAsync(string identifier)
        {
            if (_cache.ContainsKey(identifier))
            {
                await _componentStagingStore.RemoveItemAsync(identifier).ConfigureAwait(false);
                _cache.Remove(identifier);
            }
            else
            {
                throw new KeyNotFoundException($"For some reason the cache has not been populated with item with the ID '{identifier}', so the item could not be removed. This is an error condition, meaning the cache is improperly written or the request for this item is erraneous.");
            }
        }

        public async Task SetItemAsync(string identifier, Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStream.Seek(0, SeekOrigin.Begin);
                await _componentStagingStore.SetItemAsync(identifier, memoryStream).ConfigureAwait(false);
                _cache[identifier] = memoryStream.GetBuffer();
            }
        }
    }
}