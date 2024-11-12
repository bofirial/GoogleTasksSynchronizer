using GoogleTasksSynchronizer.DataAbstraction;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public class MasterTaskBusinessManager(IMasterTaskManager masterTaskManager) : IMasterTaskBusinessManager
    {
        public Task<List<MasterTask>> SelectAllAsync(string synchronizationId)
        {
            return masterTaskManager.SelectAllAsync(synchronizationId);
        }

        public Task UpdateAsync(string synchronizationId, List<MasterTask> tasks)
        {
            return masterTaskManager.UpdateAsync(synchronizationId, tasks);
        }
    }
}
