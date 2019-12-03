using GoogleTasksSynchronizer.DataAbstraction;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
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

        //private readonly IGoogleTaskAccountManager _googleTaskAccountManager;
        //private readonly ITaskChangeCalculator _taskChangeCalculator;
        //private readonly ITaskBusinessManager _taskBusinessManager;

        private readonly ITaskServiceFactory _taskServiceFactory;
        private readonly TelemetryClient _telemetryClient;

        public TaskChangesProcessor(
            ILogger<TaskChangesProcessor> logger,
            IMasterTaskGroupBusinessManager masterTaskGroupBusinessManager
            //IGoogleTaskAccountManager googleTaskAccountManager,
            //ITaskChangeCalculator taskChangeCalculator,
            //ITaskBusinessManager taskBusinessManager
            , ITaskServiceFactory taskServiceFactory
            , TelemetryConfiguration configuration
            )
        {
            _logger = logger;
            _masterTaskGroupBusinessManager = masterTaskGroupBusinessManager;
            //_googleTaskAccountManager = googleTaskAccountManager;
            //_taskChangeCalculator = taskChangeCalculator;
            //_taskBusinessManager = taskBusinessManager;
            _taskServiceFactory = taskServiceFactory;
            _telemetryClient = new TelemetryClient(configuration);
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
                                    Title = task.Title,
                                    Due = task.Due,
                                    Notes = task.Notes,
                                    Status = task.Status,
                                    Deleted = task.Deleted,
                                    Completed = task.Completed,
                                    Hidden = task.Hidden,
                                    TaskMaps = new List<TaskMap>()
                                };

                                foreach (var accountToCheck in masterTaskGroup.TaskAccountGroups)
                                {
                                    //CONSIDER: Duplicate Tasks are hidden here
                                    var matchedTask = accountToCheck.Tasks.Where(t => masterTask.Title == t.Title &&
                                                                                masterTask.Due == t.Due &&
                                                                                masterTask.Notes == t.Notes &&
                                                                                masterTask.Status == t.Status &&
                                                                                masterTask.Deleted == t.Deleted &&
                                                                                masterTask.Completed == t.Completed &&
                                                                                masterTask.Hidden == t.Hidden).FirstOrDefault();

                                    if (null == matchedTask)
                                    {
                                        var taskService = await _taskServiceFactory.CreateTaskServiceAsync(accountToCheck.SynchronizationTarget);

                                        var newTask = new Google::Task()
                                        {
                                            Title = masterTask.Title,
                                            Notes = masterTask.Notes,
                                            Due = masterTask.Due,
                                            Status = masterTask.Status,
                                            //DueRaw = masterTask.DueRaw,
                                            Deleted = masterTask.Deleted,
                                            Completed = masterTask.Completed,
                                            //CompletedRaw = masterTask.CompletedRaw,
                                        };

                                        var insertRequest = taskService.Tasks.Insert(newTask, accountToCheck.SynchronizationTarget.TaskListId);

                                        matchedTask = insertRequest.Execute();
                                        _telemetryClient.TrackEvent("GoogleAPICall");
                                        _telemetryClient.TrackEvent("CreatedTask");
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

                            //TODO: Protect from multiple clearings in a single run because each clear clears all completed tasks
                            foreach (var accountToClear in masterTaskGroup.TaskAccountGroups)
                            {
                                var taskService = await _taskServiceFactory.CreateTaskServiceAsync(accountToClear.SynchronizationTarget);

                                taskService.Tasks.Clear(accountToClear.SynchronizationTarget.TaskListId).Execute();
                                _telemetryClient.TrackEvent("GoogleAPICall");
                                _telemetryClient.TrackEvent("ClearTasks");
                            }
                        }

                        if (masterTask.Title != task.Title ||
                            masterTask.Due != task.Due ||
                            masterTask.Notes != task.Notes ||
                            masterTask.Status != task.Status ||

                            masterTask.Deleted != task.Deleted ||
                            masterTask.Completed != task.Completed)
                        {
                            masterTask.Title = task.Title;
                            masterTask.Due = task.Due;
                            masterTask.Notes = task.Notes;
                            masterTask.Status = task.Status;
                            masterTask.Deleted = task.Deleted;
                            masterTask.Completed = task.Completed;

                            foreach (var accountToCheck in masterTaskGroup.TaskAccountGroups)
                            {
                                //TODO: Handle Missing TaskMaps?
                                var targetTaskId = masterTask.TaskMaps.FirstOrDefault(tm =>
                                    tm.SynchronizationTarget.GoogleAccountName == accountToCheck.SynchronizationTarget.GoogleAccountName)?.TaskId;

                                var matchedTask = accountToCheck.Tasks.Where(t => t.Id == targetTaskId).FirstOrDefault();

                                if (masterTask.Title != matchedTask.Title ||
                                    masterTask.Due != matchedTask.Due ||
                                    masterTask.Notes != matchedTask.Notes ||
                                    masterTask.Status != matchedTask.Status ||

                                    masterTask.Deleted != matchedTask.Deleted ||
                                    masterTask.Completed != matchedTask.Completed)
                                {
                                    var taskService = await _taskServiceFactory.CreateTaskServiceAsync(accountToCheck.SynchronizationTarget);

                                    matchedTask.Title = masterTask.Title;
                                    matchedTask.Notes = masterTask.Notes;
                                    matchedTask.Due = masterTask.Due;
                                    matchedTask.Status = masterTask.Status;
                                    //matchedTask.DueRaw = masterTask.DueRaw;
                                    matchedTask.Deleted = masterTask.Deleted;
                                    matchedTask.Completed = masterTask.Completed;
                                    //matchedTask.CompletedRaw = masterTask.CompletedRaw;

                                    var updateRequest = taskService.Tasks.Update(matchedTask, accountToCheck.SynchronizationTarget.TaskListId, matchedTask.Id);

                                    updateRequest.Execute();
                                    _telemetryClient.TrackEvent("GoogleAPICall");
                                    _telemetryClient.TrackEvent("ModifiedTask");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
