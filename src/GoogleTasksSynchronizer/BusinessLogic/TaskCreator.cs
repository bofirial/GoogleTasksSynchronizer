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
    public class TaskCreator : ITaskCreator
    {
        private readonly ITaskMapper _taskMapper;
        private readonly ITaskBusinessManager _taskBusinessManager;
        private readonly ILogger<TaskCreator> _logger;

        public TaskCreator(ITaskMapper taskMapper, ITaskBusinessManager taskBusinessManager, ILogger<TaskCreator> logger)
        {
            _taskMapper = taskMapper;
            _taskBusinessManager = taskBusinessManager;
            _logger = logger;
        }

        public async Task<MasterTask> CreateNewTaskAsync(Google::Task taskToCreate, List<TaskAccountGroup> taskAccountGroups)
        {
            taskAccountGroups = taskAccountGroups ?? throw new ArgumentNullException(nameof(taskAccountGroups));

            var masterTask = new MasterTask()
            {
                MasterTaskId = Guid.NewGuid().ToString(),
                TaskMaps = new List<TaskMap>(),
                UpdatedOn = DateTime.Now
            };

            _taskMapper.MapTask(masterTask, taskToCreate);

            foreach (var taskAccountGroup in taskAccountGroups)
            {
                var equalTasks = taskAccountGroup.Tasks.Where(t => _taskBusinessManager.TasksAreEqual(masterTask, t));

                if (equalTasks.Count() > 1)
                {
                    _logger.LogWarning($"Multiple equivalent tasks named \"{masterTask.Title}\" found in google account: {taskAccountGroup.SynchronizationTarget.GoogleAccountName}");
                }

                var task = equalTasks.FirstOrDefault();

                if (null == task)
                {
                    var newTask = new Google::Task();

                    _taskMapper.MapTask(newTask, masterTask);

                    task = await _taskBusinessManager.InsertAsync(task, taskAccountGroup.SynchronizationTarget);
                }

                masterTask.TaskMaps.Add(new TaskMap()
                {
                    SynchronizationTarget = taskAccountGroup.SynchronizationTarget,
                    TaskId = task.Id
                });
            }

            return masterTask;
        }
    }
}
