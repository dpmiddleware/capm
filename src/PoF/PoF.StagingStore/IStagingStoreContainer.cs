using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.StagingStore
{
    public interface IStagingStoreContainer
    {
        //TODO: This one should probably not return all IDs, but instead allow paging or other mechanism for loading parts of the result set
        Task<Guid[]> GetStoredContextIds();
        Task<IStagingStore> GetStoreForContextIdAsync(Guid contextId);
        Task RemoveStoreForContextIdAsync(Guid contextId);
        Task CreateStoreForContextIdAsync(Guid contextId);
    }
}
