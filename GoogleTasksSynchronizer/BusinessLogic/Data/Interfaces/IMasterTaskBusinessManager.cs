using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public interface IMasterTaskBusinessManager
    {
        Task<List<MasterTask>> SelectAllAsync(string synchronizationId);

        Task UpdateAsync(string synchronizationId, List<MasterTask> tasks);
    }
}
