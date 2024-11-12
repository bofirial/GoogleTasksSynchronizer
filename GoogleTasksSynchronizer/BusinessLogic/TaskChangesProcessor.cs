using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;
using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskChangesProcessor(
        ILogger<TaskChangesProcessor> logger,
        IMasterTaskGroupBusinessManager masterTaskGroupBusinessManager,
        ITaskBusinessManager taskBusinessManager,
        IMasterTaskBusinessManager masterTaskBusinessManager,
        IDeletedTasksProcessor deletedTasksProcessor,
        ITaskCreator taskCreator,
        ITaskUpdater taskUpdater,
        ITaskSorter taskSorter
            ) : ITaskChangesProcessor
    {
        private readonly HashSet<string> _updatedMasterTasks = [];

        public async Task ProcessTaskChangesAsync()
        {
            var masterTaskGroups = await masterTaskGroupBusinessManager.SelectAsync();

            var tasks = new List<Task>();

            foreach (var masterTaskGroup in masterTaskGroups)
            {
                logger.LogInformation($"Processing Task Changes for lists with SynchronizationId: {masterTaskGroup.SynchronizationId}");

                tasks.Add(ProcessTaskChangesAsync(masterTaskGroup));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ProcessTaskChangesAsync(MasterTaskGroup masterTaskGroup)
        {
            logger.LogInformation($"{masterTaskGroup.MasterTasks.Count} master tasks stored for: {masterTaskGroup.SynchronizationId}");

            masterTaskGroup.TaskAccountGroups.ForEach(taskAccountGroup =>
                logger.LogInformation($"{taskAccountGroup.Tasks.Count} tasks to process for " +
                    $"Google Account ({taskAccountGroup.SynchronizationTarget.GoogleAccountName}) and SyncronizationId ({masterTaskGroup.SynchronizationId})"));

            var anyChanges = false;

            anyChanges |= await deletedTasksProcessor.ProcessDeletedTasksAsync(masterTaskGroup);

            foreach (var taskAccountGroup in masterTaskGroup.TaskAccountGroups)
            {
                var tasks = new List<Task<bool>>();

                foreach (var task in taskAccountGroup.Tasks.Where(taskBusinessManager.ShouldSynchronizeTask))
                {
                    tasks.Add(ProcessTaskChangeAsync(masterTaskGroup, task));
                }

                var results = await Task.WhenAll(tasks);

                anyChanges |= results.Any(r => r);
            }

            if (!anyChanges)
            {
                foreach (var taskAccountGroup in masterTaskGroup.TaskAccountGroups)
                {
                    await taskSorter.SortTasksAsync(taskAccountGroup);
                }
            }

            await masterTaskBusinessManager.UpdateAsync(masterTaskGroup.SynchronizationId, masterTaskGroup.MasterTasks);
        }

        private async Task<bool> ProcessTaskChangeAsync(MasterTaskGroup masterTaskGroup, Google::Task task)
        {
            var masterTask = masterTaskGroup.MasterTasks.Where(mt => mt.TaskMaps.Any(tm => tm.TaskId == task.Id)).FirstOrDefault();

            if (null == masterTask)
            {
                if (task.Hidden != true && task.Deleted != true)
                {
                    logger.LogInformation($"Creating task with Title ({task.Title}) for SyncronizationId ({masterTaskGroup.SynchronizationId})");

                    masterTaskGroup.MasterTasks.Add(await taskCreator.CreateNewTaskAsync(task, masterTaskGroup.TaskAccountGroups));

                    return true;
                }
            }
            else if (!taskBusinessManager.TasksAreEqual(masterTask, task) && !_updatedMasterTasks.Contains(masterTask.MasterTaskId))
            {
                logger.LogInformation($"Updating task with Title ({masterTask.Title}) for SyncronizationId ({masterTaskGroup.SynchronizationId})");

                await taskUpdater.UpdateTaskAsync(task, masterTask, masterTaskGroup.TaskAccountGroups);

                _updatedMasterTasks.Add(masterTask.MasterTaskId);

                return true;
            }

            return false;
        }
    }
}
