using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.StagingStore.Filesystem
{
    public class FileSystemStagingStore : IStagingStore
    {
        private readonly string _stagingStoreRootPath;

        public FileSystemStagingStore(string stagingStoreRootPath)
        {
            if (string.IsNullOrWhiteSpace(stagingStoreRootPath))
            {
                throw new ArgumentException("Root path for file system staging store is mandatory", nameof(stagingStoreRootPath));
            }

            _stagingStoreRootPath = stagingStoreRootPath;
        }

        public Task<IComponentStagingStore> GetComponentStagingStoreAsync(string componentIdentifier)
        {
            var componentStagingStorePath = Path.Combine(_stagingStoreRootPath, componentIdentifier);
            if (!Directory.Exists(componentStagingStorePath))
            {
                Directory.CreateDirectory(componentStagingStorePath);
            }
            return Task.FromResult<IComponentStagingStore>(new FileSystemComponentStagingStore(componentStagingStorePath));
        }
    }
}
