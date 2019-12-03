using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface IMasterTaskManager
    {
        Task<List<MasterTask>> SelectAllAsync(string synchronizationId);

        Task UpdateAsync(string synchronizationId, List<MasterTask> tasks);
    }
}
