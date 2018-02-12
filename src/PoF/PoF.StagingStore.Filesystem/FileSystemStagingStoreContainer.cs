using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.StagingStore.Filesystem
{
    public class FileSystemStagingStoreContainer : IStagingStoreContainer
    {
        private readonly string _stagingStoreContainerRootPath;

        public FileSystemStagingStoreContainer(string stagingStoreContainerRootPath)
        {
            if (string.IsNullOrWhiteSpace(stagingStoreContainerRootPath))
            {
                throw new ArgumentException("Root path for the staging store container is mandatory", nameof(stagingStoreContainerRootPath));
            }

            _stagingStoreContainerRootPath = stagingStoreContainerRootPath;
        }

        public Task CreateStoreForContextIdAsync(Guid contextId)
        {
            var stagingStoreRootPath = GetStagingStorePath(contextId);
            if (Directory.Exists(stagingStoreRootPath))
            {
                throw new Exception("Cannot create a staging store for a context ID which already has a staging store");
            }
            else
            {
                Directory.CreateDirectory(stagingStoreRootPath);
                return Task.CompletedTask;
            }
        }

        public Task<Guid[]> GetStoredContextIds()
        {
            return Task.FromResult(Directory.EnumerateDirectories(_stagingStoreContainerRootPath).Select(directory => new Guid(Path.GetFileName(directory))).ToArray());
        }

        public Task<IStagingStore> GetStoreForContextIdAsync(Guid contextId)
        {
            return Task.FromResult<IStagingStore>(new FileSystemStagingStore(GetStagingStorePath(contextId)));
        }

        public Task RemoveStoreForContextIdAsync(Guid contextId)
        {
            Directory.Delete(GetStagingStorePath(contextId));
            return Task.CompletedTask;
        }

        private string GetStagingStorePath(Guid contextId)
        {
            return Path.Combine(_stagingStoreContainerRootPath, contextId.ToString());
        }
    }
}
