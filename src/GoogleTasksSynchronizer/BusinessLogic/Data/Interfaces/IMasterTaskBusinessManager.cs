using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public interface IMasterTaskBusinessManager
    {
        Task<List<MasterTask>> SelectAllAsync(string synchronizationId);

        Task UpdateAsync(string synchronizationId, List<MasterTask> tasks);
    }
}
