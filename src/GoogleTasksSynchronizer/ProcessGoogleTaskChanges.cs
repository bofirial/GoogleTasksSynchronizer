using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.BusinessLogic;
using GoogleTasksSynchronizer.DataAbstraction;
using GoogleTasksSynchronizer.Google;
using GoogleTasksSynchronizer.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GoogleTasksSynchronizer
{
    public class ProcessGoogleTaskChanges
    {
        private readonly ILogger _logger;
        private readonly ITasksSynchronizerStateManager _tasksSynchronizerStateManager;
        private readonly ITaskBusinessManager _taskBusinessManager;
        private readonly IGoogleTaskAccountManager _googleTaskAccountManager;

        public ProcessGoogleTaskChanges(
            ILogger logger, 
            ITasksSynchronizerStateManager tasksSynchronizerStateManager,
            ITaskBusinessManager taskBusinessManager,
            IGoogleTaskAccountManager googleTaskAccountManager)
        {
            _logger = logger;
            _tasksSynchronizerStateManager = tasksSynchronizerStateManager;
            _taskBusinessManager = taskBusinessManager;
            _googleTaskAccountManager = googleTaskAccountManager;
        }

        [FunctionName("ProcessGoogleTaskChanges")]
        public async System.Threading.Tasks.Task Run(
            [TimerTrigger("*/15 * 6-23 * * *", RunOnStartup = true)]TimerInfo myTimer, 
            [Blob("jschaferfunctions/googleTasksSynchronizerState.json", Connection = "AzureWebJobsStorage")] CloudBlockBlob googleTasksSynchronizerState)
        {
            _logger.LogInformation($"ProcessGoogleTaskChanges Timer trigger function started at: {DateTime.Now}");

            var tasksSynchronizerState = await _tasksSynchronizerStateManager.SelectTasksSynchronizerStateAsync(googleTasksSynchronizerState);

            var taskAccounts = await _googleTaskAccountManager.GetTaskAccountsAsync(tasksSynchronizerState);

            var createdTasks = new List<Task>();
            var modifiedTasks = new List<Task>();
            var clearedTasks = new List<Task>();

            foreach (var taskAccount in taskAccounts)
            {
                CalculateTaskChanges(tasksSynchronizerState, createdTasks, modifiedTasks, clearedTasks, taskAccount);
            }

            _logger.LogInformation($"{createdTasks.Count} tasks to create.");

            foreach (var createdTask in createdTasks)
            {
                var taskIdentifiers = new List<TaskIdentifier>();

                foreach (var taskAccount in taskAccounts)
                {
                    var taskService = new TasksService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = taskAccount.UserCredential,
                        ApplicationName = "JSchafer Google Tasks Synchronizer",
                    });


                    var tasks = taskAccount.GoogleTasks?.Where(t => _taskBusinessManager.TasksAreLogicallyEqual(createdTask, t)).ToArray();

                    if (null == tasks || !tasks.Any())
                    {
                        _logger.LogInformation($"\tNew Task \"{createdTask.Title}\" for {taskAccount.AccountName}.");

                        var newTask = new Task()
                        {
                            Title = createdTask.Title,
                            Notes = createdTask.Notes,
                            Due = createdTask.Due,
                            Status = createdTask.Status,
                            DueRaw = createdTask.DueRaw,
                            Deleted = createdTask.Deleted,
                            Completed = createdTask.Completed,
                            CompletedRaw = createdTask.CompletedRaw,
                        };

                        var insertRequest = taskService.Tasks.Insert(newTask, taskAccount.TaskListId);

                        var createdGoogleTask = insertRequest.Execute();

                        taskIdentifiers.Add(new TaskIdentifier() { AccountName = taskAccount.AccountName, TaskId = createdGoogleTask.Id });
                    }
                    else
                    {
                        taskIdentifiers.AddRange(tasks.Select(t => new TaskIdentifier() { AccountName = taskAccount.AccountName, TaskId = t.Id }));
                    }
                }

                tasksSynchronizerState.CurrentTasks.Add(new CurrentTask()
                {
                    Task = createdTask,
                    TaskIds = taskIdentifiers
                });
            }

            _logger.LogInformation($"{modifiedTasks.Count} tasks to modify.");

            foreach (var modifiedTask in modifiedTasks)
            {
                var currentTasks = tasksSynchronizerState.CurrentTasks.Where(c => c.TaskIds.Any(t => modifiedTask.Id == t.TaskId)).ToList();

                foreach (var currentTask in currentTasks)
                {
                    currentTask.Task = modifiedTask;
                }

                foreach (var taskAccount in taskAccounts)
                {
                    var taskService = new TasksService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = taskAccount.UserCredential,
                        ApplicationName = "JSchafer Google Tasks Synchronizer",
                    });

                    var taskIdsToModify = currentTasks.SelectMany(c => c.TaskIds).Where(c => c.AccountName == taskAccount.AccountName).Select(t => t.TaskId).ToList();

                    foreach (var taskId in taskIdsToModify)
                    {

                        var taskToModify = taskAccount.GoogleTasks?.FirstOrDefault(t => t.Id == taskId);

                        if (null == taskToModify)
                        {
                            taskToModify = taskService.Tasks.Get(taskAccount.TaskListId, taskId).Execute();
                        }

                        if (_taskBusinessManager.TasksAreLogicallyEqual(taskToModify, modifiedTask))
                        {
                            continue;
                        }

                        _logger.LogInformation($"\tModifying Task \"{taskToModify.Title}\" for {taskAccount.AccountName}.");

                        taskToModify.Title = modifiedTask.Title;
                        taskToModify.Notes = modifiedTask.Notes;
                        taskToModify.Due = modifiedTask.Due;
                        taskToModify.Status = modifiedTask.Status;
                        taskToModify.DueRaw = modifiedTask.DueRaw;
                        taskToModify.Deleted = modifiedTask.Deleted;
                        taskToModify.Completed = modifiedTask.Completed;
                        taskToModify.CompletedRaw = modifiedTask.CompletedRaw;

                        var updateRequest = taskService.Tasks.Update(taskToModify, taskAccount.TaskListId, taskToModify.Id);

                        updateRequest.Execute();
                    }
                }
            }

            if (clearedTasks.Any())
            {
                _logger.LogInformation("Tasks must be cleared.");

                foreach (var taskAccount in taskAccounts)
                {
                    _logger.LogInformation($"\tClearing Tasks for {taskAccount.AccountName}.");
                    var taskService = new TasksService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = taskAccount.UserCredential,
                        ApplicationName = "JSchafer Google Tasks Synchronizer",
                    });

                    taskService.Tasks.Clear(taskAccount.TaskListId).Execute();
                }

                foreach (var clearedTask in clearedTasks)
                {
                    var storedTask = _taskBusinessManager.GetStoredTaskById(clearedTask.Id, tasksSynchronizerState);

                    storedTask.Hidden = clearedTask.Hidden;
                }
            }

            tasksSynchronizerState.CurrentTasks.RemoveAll(c =>
                c.Task.Updated < DateTime.Today.AddDays(-7) && (c.Task.Hidden == true || c.Task.Deleted == true));

            await _tasksSynchronizerStateManager.UpdateTasksSynchronizerStateAsync(tasksSynchronizerState, googleTasksSynchronizerState);

            _logger.LogInformation($"ProcessGoogleTaskChanges Timer trigger function completed at: {DateTime.Now}");
        }

        private void CalculateTaskChanges(TasksSynchronizerState tasksSynchronizerState, List<Task> createdTasks, List<Task> modifiedTasks, List<Task> clearedTasks, TaskAccount taskAccount)
        {
            var taskService = new TasksService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = taskAccount.UserCredential,
                ApplicationName = "JSchafer Google Tasks Synchronizer",
            });

            var listRequest = taskService.Tasks.List(taskAccount.TaskListId);

            listRequest.ShowCompleted = true;
            listRequest.ShowDeleted = true;
            listRequest.ShowHidden = true;

            listRequest.UpdatedMin = DateTime.Today.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

            taskAccount.GoogleTasks = _taskBusinessManager.RequestAllGoogleTasks(listRequest);

            _logger.LogInformation($"{taskAccount.GoogleTasks.Count} updated tasks today for {taskAccount.AccountName}");

            foreach (var task in taskAccount.GoogleTasks)
            {
                var storedTask = _taskBusinessManager.GetStoredTaskById(task.Id, tasksSynchronizerState);

                if (storedTask != null && _taskBusinessManager.TasksMustBeCleared(task, storedTask))
                {
                    clearedTasks.Add(task);
                }

                if (null == storedTask)
                {
                    if (task.Hidden != true && task.Deleted != true)
                    {
                        createdTasks.Add(task);
                    }

                    continue;
                }

                if (!_taskBusinessManager.TasksAreLogicallyEqual(task, storedTask))
                {
                    modifiedTasks.Add(task);
                    continue;
                }
            }
        }
    }
}
