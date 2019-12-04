using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;
using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskUpdater : ITaskUpdater
    {
        private readonly ITaskMapper _taskMapper;
        private readonly ITaskBusinessManager _taskBusinessManager;
        private readonly ILogger<TaskUpdater> _logger;

        public TaskUpdater(ITaskMapper taskMapper, ITaskBusinessManager taskBusinessManager, ILogger<TaskUpdater> logger)
        {
            _taskMapper = taskMapper;
            _taskBusinessManager = taskBusinessManager;
            _logger = logger;
        }

        public async Task UpdateTaskAsync(Google::Task task, MasterTask masterTask, List<TaskAccountGroup> taskAccountGroups)
        {
            masterTask = masterTask ?? throw new ArgumentNullException(nameof(masterTask));
            taskAccountGroups = taskAccountGroups ?? throw new ArgumentNullException(nameof(taskAccountGroups));

            _taskMapper.MapTask(masterTask, task);
            masterTask.UpdatedOn = DateTime.Now;

            foreach (var taskAccountGroup in taskAccountGroups)
            {
                await UpdateTaskAccountGroupTaskAsync(masterTask, taskAccountGroup);
            }
        }

        private async Task UpdateTaskAccountGroupTaskAsync(MasterTask masterTask, TaskAccountGroup taskAccountGroup)
        {
            var taskToUpdateId = masterTask.TaskMaps.FirstOrDefault(tm =>
                tm.SynchronizationTarget.GoogleAccountName == taskAccountGroup.SynchronizationTarget.GoogleAccountName)?.TaskId;

            var taskToUpdate = taskAccountGroup.Tasks.Where(t => t.Id == taskToUpdateId).FirstOrDefault();

            if (null == taskToUpdate)
            {
                _logger.LogError($"Missing Task Mapping for Task: \"{masterTask.Title}\"");
            }
            else if (!_taskBusinessManager.TasksAreEqual(masterTask, taskToUpdate))
            {
                _taskMapper.MapTask(taskToUpdate, masterTask);

                await _taskBusinessManager.UpdateAsync(taskToUpdate, taskAccountGroup.SynchronizationTarget);
            }
        }
    }
}
