using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface IDeletedTasksProcessor
    {
        Task<bool> ProcessDeletedTasksAsync(MasterTaskGroup masterTaskGroup);
    }
}
