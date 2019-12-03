using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskClearer : ITaskClearer
    {
        private readonly ITaskBusinessManager _taskBusinessManager;

        public TaskClearer(ITaskBusinessManager taskBusinessManager)
        {
            _taskBusinessManager = taskBusinessManager;
        }

        public async Task ClearTasksAsync(List<TaskAccountGroup> taskAccountGroups)
        {
            foreach (var accountToClear in taskAccountGroups)
            {
                await _taskBusinessManager.ClearAsync(accountToClear.SynchronizationTarget);
            }
        }
    }
}
