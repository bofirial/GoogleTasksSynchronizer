using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskChangesProcessor : ITaskChangesProcessor
    {
        private readonly ILogger<TaskChangesProcessor> _logger;
        private readonly IMasterTaskGroupBusinessManager _masterTaskGroupBusinessManager;
        private readonly ITaskBusinessManager _taskBusinessManager;
        private readonly IMasterTaskBusinessManager _masterTaskBusinessManager;
        private readonly ITaskCreator _taskCreator;
        private readonly ITaskUpdater _taskUpdater;
        private readonly ITaskClearer _taskClearer;

        private readonly HashSet<string> _updatedMasterTasks = new HashSet<string>();

        public TaskChangesProcessor(
            ILogger<TaskChangesProcessor> logger,
            IMasterTaskGroupBusinessManager masterTaskGroupBusinessManager,
            ITaskBusinessManager taskBusinessManager,
            IMasterTaskBusinessManager masterTaskBusinessManager,
            ITaskCreator taskCreator,
            ITaskUpdater taskUpdater,
            ITaskClearer taskClearer
            )
        {
            _logger = logger;
            _masterTaskGroupBusinessManager = masterTaskGroupBusinessManager;
            _taskBusinessManager = taskBusinessManager;
            _masterTaskBusinessManager = masterTaskBusinessManager;
            _taskCreator = taskCreator;
            _taskUpdater = taskUpdater;
            _taskClearer = taskClearer;
        }

        public async Task ProcessTaskChangesAsync()
        {
            var masterTaskGroups = await _masterTaskGroupBusinessManager.SelectAsync();

            var tasks = new List<Task>();

            foreach (var masterTaskGroup in masterTaskGroups)
            {
                _logger.LogInformation($"Processing Task Changes for lists with SynchronizationId: {masterTaskGroup.SynchronizationId}");

                tasks.Add(ProcessTaskChangesAsync(masterTaskGroup));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ProcessTaskChangesAsync(MasterTaskGroup masterTaskGroup)
        {
            _logger.LogInformation($"{masterTaskGroup.MasterTasks.Count} master tasks stored for: {masterTaskGroup.SynchronizationId}");

            foreach (var taskAccountGroup in masterTaskGroup.TaskAccountGroups)
            {
                _logger.LogInformation($"{masterTaskGroup.MasterTasks.Count} tasks to process for " +
                    $"Google Account ({taskAccountGroup.SynchronizationTarget}) and SyncronizationId ({masterTaskGroup.SynchronizationId})");

                var tasks = new List<Task>();

                foreach (var task in taskAccountGroup.Tasks)
                {
                    tasks.Add(ProcessTaskChangeAsync(masterTaskGroup, task));
                }

                await Task.WhenAll(tasks);
            }

            await _masterTaskBusinessManager.UpdateAsync(masterTaskGroup.SynchronizationId, masterTaskGroup.MasterTasks);
        }

        private async Task ProcessTaskChangeAsync(MasterTaskGroup masterTaskGroup, Google::Task task)
        {
            var masterTask = masterTaskGroup.MasterTasks.Where(mt => mt.TaskMaps.Any(tm => tm.TaskId == task.Id)).FirstOrDefault();

            if (null == masterTask)
            {
                if (task.Hidden != true && task.Deleted != true)
                {
                    _logger.LogInformation($"Creating task with Title ({task.Title}) for SyncronizationId ({masterTaskGroup.SynchronizationId})");

                    masterTaskGroup.MasterTasks.Add(await _taskCreator.CreateNewTaskAsync(task, masterTaskGroup.TaskAccountGroups));
                }
            }
            else
            {
                if (masterTask.Hidden != task.Hidden)
                {
                    masterTask.Hidden = task.Hidden;

                    await _taskClearer.ClearTasksAsync(masterTaskGroup.TaskAccountGroups);
                }

                if (!_taskBusinessManager.TasksAreEqual(masterTask, task) && !_updatedMasterTasks.Contains(masterTask.MasterTaskId))
                {
                    _logger.LogInformation($"Updating task with Title ({masterTask.Title}) for SyncronizationId ({masterTaskGroup.SynchronizationId})");

                    await _taskUpdater.UpdateTaskAsync(task, masterTask, masterTaskGroup.TaskAccountGroups);

                    _updatedMasterTasks.Add(masterTask.MasterTaskId);
                }
            }
        }
    }
}
