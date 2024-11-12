using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskDeleter(
        ILogger<TaskDeleter> logger,
        ITaskBusinessManager taskBusinessManager
            ) : ITaskDeleter
    {
        public async Task DeleteTaskAsync(MasterTask masterTask, List<TaskAccountGroup> taskAccountGroups)
        {
            masterTask = masterTask ?? throw new ArgumentNullException(nameof(masterTask));

            logger.LogInformation($"Deleting task with Title ({masterTask.Title}) for SyncronizationId ({taskAccountGroups.First().SynchronizationTarget.SynchronizationId})");

            foreach (var taskMap in masterTask.TaskMaps)
            {
                var taskAccountGroup = taskAccountGroups.First(t => t.SynchronizationTarget.GoogleAccountName == taskMap.SynchronizationTarget.GoogleAccountName);

                var task = taskAccountGroup.Tasks.FirstOrDefault(t => t.Id == taskMap.TaskId);

                if (null != task)
                {
                    task.Deleted = true;

                    await taskBusinessManager.UpdateAsync(task, taskAccountGroup.SynchronizationTarget);
                }

                taskAccountGroup.Tasks.RemoveAll(t => t.Id == taskMap.TaskId);
            }
        }
    }
}
