using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskCreator : ITaskCreator
    {
        private readonly ITaskMapper _taskMapper;
        private readonly ITaskBusinessManager _taskBusinessManager;

        public TaskCreator(ITaskMapper taskMapper, ITaskBusinessManager taskBusinessManager)
        {
            _taskMapper = taskMapper;
            _taskBusinessManager = taskBusinessManager;
        }

        public async Task<MasterTask> CreateNewTaskAsync(Google::Task task, List<TaskAccountGroup> taskAccountGroups)
        {
            var masterTask = new MasterTask()
            {
                MasterTaskId = Guid.NewGuid().ToString(),
                TaskMaps = new List<TaskMap>(),
                UpdatedOn = DateTime.Now
            };

            _taskMapper.MapTask(masterTask, task);

            foreach (var accountToCheck in taskAccountGroups)
            {
                //CONSIDER: Duplicate Tasks are hidden here
                var matchedTask = accountToCheck.Tasks.Where(t => _taskBusinessManager.TasksAreEqual(masterTask, t)).FirstOrDefault();

                if (null == matchedTask)
                {
                    var newTask = new Google::Task();

                    _taskMapper.MapTask(newTask, masterTask);

                    matchedTask = await _taskBusinessManager.InsertAsync(matchedTask, accountToCheck.SynchronizationTarget);
                }

                masterTask.TaskMaps.Add(new TaskMap()
                {
                    SynchronizationTarget = accountToCheck.SynchronizationTarget,
                    TaskId = matchedTask.Id
                });
            }

            return masterTask;
        }
    }
}
