using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface IMasterTaskManager
    {
        Task<List<MasterTask>> SelectAllAsync(string synchronizationId);

        Task UpdateAsync(string synchronizationId, List<MasterTask> tasks);
    }
}
