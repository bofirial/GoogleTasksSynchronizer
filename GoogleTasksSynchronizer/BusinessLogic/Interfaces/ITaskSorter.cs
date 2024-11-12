using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskSorter
    {
        Task SortTasksAsync(TaskAccountGroup taskAccountGroup);
    }
}
