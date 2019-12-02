using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public class TaskBusinessManager : ITaskBusinessManager
    {
        private readonly ITaskManager _taskManager;

        public TaskBusinessManager(
            ITaskManager taskManager
            )
        {
            _taskManager = taskManager;
        }

        public async Task<List<Google::Task>> SelectAllAsync(SynchronizationTarget synchronizationTarget)
        {
            return await _taskManager.SelectAllAsync(synchronizationTarget);
        }
    }
}
