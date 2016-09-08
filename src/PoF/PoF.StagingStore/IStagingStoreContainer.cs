using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.StagingStore
{
    public interface IStagingStoreContainer
    {
        Task<IStagingStore> GetStoreForContextIdAsync(Guid contextId);
        Task RemoveStoreForContextIdAsync(Guid contextId);
        Task CreateStoreForContextIdAsync(Guid contextId);
    }
}
