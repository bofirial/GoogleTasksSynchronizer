
using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskClearer
    {
        Task ClearTasksAsync(List<TaskAccountGroup> taskAccountGroups);
    }
}
