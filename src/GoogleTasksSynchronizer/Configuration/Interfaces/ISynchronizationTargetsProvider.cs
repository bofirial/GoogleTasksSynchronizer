using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.Configuration
{
    public interface ISynchronizationTargetsProvider
    {
        Task<List<SynchronizationTarget>> GetAsync();
    }
}
