using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PoF.StagingStore.Filesystem
{
    internal class FileSystemComponentStagingStore : IComponentStagingStore
    {
        private string _componentStagingStorePath;

        public FileSystemComponentStagingStore(string componentStagingStorePath)
        {
            _componentStagingStorePath = componentStagingStorePath;
        }

        public Task<string[]> GetAvailableIdentifiersAsync()
        {
            return Task.FromResult(Directory.EnumerateFiles(_componentStagingStorePath).Select(file => Path.GetFileName(file)).ToArray());
        }

        public Task<Stream> GetItemAsync(string identifier)
        {
            return Task.FromResult<Stream>(File.Open(GetPathByIdentifier(identifier), FileMode.Open));
        }

        public Task<bool> HasItemAsync(string identifier)
        {
            return Task.FromResult(File.Exists(GetPathByIdentifier(identifier)));
        }

        public Task RemoveItemAsync(string identifier)
        {
            File.Delete(GetPathByIdentifier(identifier));
            return Task.CompletedTask;
        }

        public async Task SetItemAsync(string identifier, Stream stream)
        {
            using (var fileStream = File.OpenWrite(GetPathByIdentifier(identifier)))
            {
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }
        }

        private string GetPathByIdentifier(string identifier)
        {
            return Path.Combine(_componentStagingStorePath, identifier);
        }
    }
}