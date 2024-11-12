using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class DeletedTasksProcessor(
        ILogger<DeletedTasksProcessor> logger,
        ITaskDeleter taskDeleter
            ) : IDeletedTasksProcessor
    {
        public async Task<bool> ProcessDeletedTasksAsync(MasterTaskGroup masterTaskGroup)
        {
            masterTaskGroup = masterTaskGroup ?? throw new ArgumentNullException(nameof(masterTaskGroup));

            var deletedTaskIds = new HashSet<string>();

            foreach (var masterTask in masterTaskGroup.MasterTasks)
            {
                foreach (var taskMap in masterTask.TaskMaps)
                {
                    var taskGroup = masterTaskGroup.TaskAccountGroups
                                    .First(t => t.SynchronizationTarget.GoogleAccountName == taskMap.SynchronizationTarget.GoogleAccountName);

                    if (!taskGroup.Tasks.Any(t => t.Id == taskMap.TaskId))
                    {
                        logger.LogInformation($"\"{masterTask.Title}\" task deleted from Google Account ({taskGroup.SynchronizationTarget.GoogleAccountName}) for SyncronizationId ({masterTaskGroup.SynchronizationId}).");

                        deletedTaskIds.Add(masterTask.MasterTaskId);

                        break;
                    }
                }
            }

            foreach (var deletedTaskId in deletedTaskIds)
            {
                var masterTask = masterTaskGroup.MasterTasks.First(m => m.MasterTaskId == deletedTaskId);

                await taskDeleter.DeleteTaskAsync(masterTask, masterTaskGroup.TaskAccountGroups);

                masterTaskGroup.MasterTasks.RemoveAll(m => m.MasterTaskId == deletedTaskId);
            }

            return deletedTaskIds.Count > 0;
        }
    }
}
