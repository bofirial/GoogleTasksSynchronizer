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
        private readonly ITaskMapper _taskMapper;

        public TaskChangesProcessor(
            ILogger<TaskChangesProcessor> logger,
            IMasterTaskGroupBusinessManager masterTaskGroupBusinessManager,
            ITaskBusinessManager taskBusinessManager,
            ITaskMapper taskMapper
            )
        {
            _logger = logger;
            _masterTaskGroupBusinessManager = masterTaskGroupBusinessManager;
            _taskBusinessManager = taskBusinessManager;
            _taskMapper = taskMapper;
        }

        public async Task ProcessTaskChangesAsync()
        {
            var masterTaskGroups = await _masterTaskGroupBusinessManager.SelectAsync();

            foreach (var masterTaskGroup in masterTaskGroups)
            {
                foreach (var taskAccountGroup in masterTaskGroup.TaskAccountGroups)
                {
                    foreach (var task in taskAccountGroup.Tasks)
                    {
                        var masterTask = masterTaskGroup.MasterTasks.Where(mt => mt.TaskMaps.Any(tm => tm.TaskId == task.Id)).FirstOrDefault();

                        if (null == masterTask)
                        {
                            if (task.Hidden != true && task.Deleted != true)
                            {
                                masterTask = new MasterTask()
                                {
                                    TaskMaps = new List<TaskMap>()
                                };

                                _taskMapper.MapTask(task, masterTask);

                                foreach (var accountToCheck in masterTaskGroup.TaskAccountGroups)
                                {
                                    //CONSIDER: Duplicate Tasks are hidden here
                                    var matchedTask = accountToCheck.Tasks.Where(t => _taskBusinessManager.TasksAreEqual(masterTask, t)).FirstOrDefault();

                                    if (null == matchedTask)
                                    {
                                        var newTask = new Google::Task();

                                        _taskMapper.MapTask(masterTask, newTask);

                                        matchedTask = await _taskBusinessManager.InsertAsync(matchedTask, accountToCheck.SynchronizationTarget);
                                    }

                                    masterTask.TaskMaps.Add(new TaskMap()
                                    {
                                        SynchronizationTarget = accountToCheck.SynchronizationTarget,
                                        TaskId = matchedTask.Id
                                    });
                                }

                                masterTaskGroup.MasterTasks.Add(masterTask);
                            }

                            continue;
                        }

                        if (masterTask.Hidden != task.Hidden)
                        {
                            masterTask.Hidden = task.Hidden;

                            foreach (var accountToClear in masterTaskGroup.TaskAccountGroups)
                            {
                                await _taskBusinessManager.ClearAsync(accountToClear.SynchronizationTarget);
                            }
                        }

                        if (!_taskBusinessManager.TasksAreEqual(masterTask, task))
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
                    }
                }
            }
        }
    }
}
