using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskDeleter : ITaskDeleter
    {
        private readonly ILogger<TaskDeleter> _logger;
        private readonly ITaskBusinessManager _taskBusinessManager;

        public TaskDeleter(
            ILogger<TaskDeleter> logger,
            ITaskBusinessManager taskBusinessManager
            )
        {
            _logger = logger;
            _taskBusinessManager = taskBusinessManager;
        }

        public async Task DeleteTaskAsync(MasterTask masterTask, List<TaskAccountGroup> taskAccountGroups)
        {
            masterTask = masterTask ?? throw new ArgumentNullException(nameof(masterTask));

            _logger.LogInformation($"Deleting task with Title ({masterTask.Title}) for SyncronizationId ({taskAccountGroups.First().SynchronizationTarget.SynchronizationId})");

            foreach (var taskMap in masterTask.TaskMaps)
            {
                var taskAccountGroup = taskAccountGroups.First(t => t.SynchronizationTarget.GoogleAccountName == taskMap.SynchronizationTarget.GoogleAccountName);

                var task = taskAccountGroup.Tasks.FirstOrDefault(t => t.Id == taskMap.TaskId);

                if (null != task)
                {
                    task.Deleted = true;

                    await _taskBusinessManager.UpdateAsync(task, taskAccountGroup.SynchronizationTarget);
                }

                taskAccountGroup.Tasks.RemoveAll(t => t.Id == taskMap.TaskId);
            }
        }
    }
}
