using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly ITaskMapper _taskMapper;
        private readonly IMasterTaskBusinessManager _masterTaskBusinessManager;
        private readonly HashSet<string> _updatedMasterTasks = new HashSet<string>();

        public TaskChangesProcessor(
            ILogger<TaskChangesProcessor> logger,
            IMasterTaskGroupBusinessManager masterTaskGroupBusinessManager,
            ITaskBusinessManager taskBusinessManager,
            ITaskMapper taskMapper,
            IMasterTaskBusinessManager masterTaskBusinessManager
            )
        {
            _logger = logger;
            _masterTaskGroupBusinessManager = masterTaskGroupBusinessManager;
            _taskBusinessManager = taskBusinessManager;
            _taskMapper = taskMapper;
            _masterTaskBusinessManager = masterTaskBusinessManager;
        }

        public async Task ProcessTaskChangesAsync()
        {
            var masterTaskGroups = await _masterTaskGroupBusinessManager.SelectAsync();

            foreach (var masterTaskGroup in masterTaskGroups)
            {
                await ProcessTaskChangesAsync(masterTaskGroup);
            }
        }

        private async Task ProcessTaskChangesAsync(MasterTaskGroup masterTaskGroup)
        {
            foreach (var taskAccountGroup in masterTaskGroup.TaskAccountGroups)
            {
                foreach (var task in taskAccountGroup.Tasks)
                {
                    await ProcessTaskChangeAsync(masterTaskGroup, task);
                }
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
                    await CreateNewTaskAsync(masterTaskGroup, task);
                }
            }
            else
            {
                if (masterTask.Hidden != task.Hidden)
                {
                    masterTask.Hidden = task.Hidden;

                    await ClearTasksAsync(masterTaskGroup);
                }

                if (!_taskBusinessManager.TasksAreEqual(masterTask, task) && !_updatedMasterTasks.Contains(masterTask.MasterTaskId))
                {
                    await UpdateTaskAsync(masterTaskGroup, task, masterTask);

                    _updatedMasterTasks.Add(masterTask.MasterTaskId);
                }
            }
        }

        private async Task CreateNewTaskAsync(MasterTaskGroup masterTaskGroup, Google::Task task)
        {
            var newMasterTask = new MasterTask()
            {
                MasterTaskId = Guid.NewGuid().ToString(),
                TaskMaps = new List<TaskMap>()
            };

            _taskMapper.MapTask(task, newMasterTask);

            foreach (var accountToCheck in masterTaskGroup.TaskAccountGroups)
            {
                //CONSIDER: Duplicate Tasks are hidden here
                var matchedTask = accountToCheck.Tasks.Where(t => _taskBusinessManager.TasksAreEqual(newMasterTask, t)).FirstOrDefault();

                if (null == matchedTask)
                {
                    var newTask = new Google::Task();

                    _taskMapper.MapTask(newMasterTask, newTask);

                    matchedTask = await _taskBusinessManager.InsertAsync(matchedTask, accountToCheck.SynchronizationTarget);
                }

                newMasterTask.TaskMaps.Add(new TaskMap()
                {
                    SynchronizationTarget = accountToCheck.SynchronizationTarget,
                    TaskId = matchedTask.Id
                });
            }

            masterTaskGroup.MasterTasks.Add(newMasterTask);
        }

        private async Task UpdateTaskAsync(MasterTaskGroup masterTaskGroup, Google::Task task, MasterTask masterTask)
        {
            _taskMapper.MapTask(task, masterTask);

            foreach (var accountToCheck in masterTaskGroup.TaskAccountGroups)
            {
                //TODO: Handle Missing TaskMaps?
                var targetTaskId = masterTask.TaskMaps.FirstOrDefault(tm =>
                    tm.SynchronizationTarget.GoogleAccountName == accountToCheck.SynchronizationTarget.GoogleAccountName)?.TaskId;

                var matchedTask = accountToCheck.Tasks.Where(t => t.Id == targetTaskId).FirstOrDefault();

                if (!_taskBusinessManager.TasksAreEqual(masterTask, matchedTask))
                {
                    _taskMapper.MapTask(masterTask, matchedTask);

                    await _taskBusinessManager.UpdateAsync(matchedTask, accountToCheck.SynchronizationTarget);
                }
            }
        }

        private async Task ClearTasksAsync(MasterTaskGroup masterTaskGroup)
        {
            foreach (var accountToClear in masterTaskGroup.TaskAccountGroups)
            {
                await _taskBusinessManager.ClearAsync(accountToClear.SynchronizationTarget);
            }
        }
    }
}
