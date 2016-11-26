using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.StagingStore
{
    public static class IComponentStagingStoreExtensions
    {
        public static Task SetItemAsync<T>(this IComponentStagingStore store, string identifier, T item)
        {
            return store.SetItemAsync(identifier, new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item))));
        }

        public static async Task<T> GetItemAsync<T>(this IComponentStagingStore store, string identifier)
        {
            var stream = await store.GetItemAsync(identifier);
            using (var streamReader = new StreamReader(stream))
            {
                var text = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(text);
            }
        }
    }
}
