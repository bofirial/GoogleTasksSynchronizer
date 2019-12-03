using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskUpdater : ITaskUpdater
    {
        private readonly ITaskMapper _taskMapper;
        private readonly ITaskBusinessManager _taskBusinessManager;

        public TaskUpdater(ITaskMapper taskMapper, ITaskBusinessManager taskBusinessManager)
        {
            _taskMapper = taskMapper;
            _taskBusinessManager = taskBusinessManager;
        }

        public async Task UpdateTaskAsync(Google::Task task, MasterTask masterTask, List<TaskAccountGroup> taskAccountGroups)
        {
            masterTask = masterTask ?? throw new ArgumentNullException(nameof(masterTask));
            taskAccountGroups = taskAccountGroups ?? throw new ArgumentNullException(nameof(taskAccountGroups));

            _taskMapper.MapTask(masterTask, task);
            masterTask.UpdatedOn = DateTime.Now;

            foreach (var accountToCheck in taskAccountGroups)
            {
                //TODO: Handle Missing TaskMaps?
                var targetTaskId = masterTask.TaskMaps.FirstOrDefault(tm =>
                    tm.SynchronizationTarget.GoogleAccountName == accountToCheck.SynchronizationTarget.GoogleAccountName)?.TaskId;

                var matchedTask = accountToCheck.Tasks.Where(t => t.Id == targetTaskId).FirstOrDefault();

                if (!_taskBusinessManager.TasksAreEqual(masterTask, matchedTask))
                {
                    _taskMapper.MapTask(matchedTask, masterTask);

                    await _taskBusinessManager.UpdateAsync(matchedTask, accountToCheck.SynchronizationTarget);
                }
            }
        }
    }
}
