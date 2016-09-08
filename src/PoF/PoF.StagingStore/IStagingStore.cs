using System.Threading.Tasks;

namespace PoF.StagingStore
{
    public interface IStagingStore
    {
        Task<IComponentStagingStore> GetComponentStagingStoreAsync(string componentIdentifier);
    }
}