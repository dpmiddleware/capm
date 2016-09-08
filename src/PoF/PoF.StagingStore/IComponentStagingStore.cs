using System.IO;
using System.Threading.Tasks;

namespace PoF.StagingStore
{
    public interface IComponentStagingStore
    {
        Task<bool> HasItemAsync(string identifier);
        Task<Stream> GetItemAsync(string identifier);
        Task SetItemAsync(string identifier, Stream stream);
        Task<string[]> GetAvailableIdentifiersAsync();
        Task RemoveItemAsync(string identifier);
    }
}