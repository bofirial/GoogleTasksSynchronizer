using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using GoogleTasksSynchronizer.Google;
using GoogleTasksSynchronizer.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskChangesProcessor : ITaskChangesProcessor
    {
        private readonly ILogger _logger;
        private readonly IGoogleTaskAccountManager _googleTaskAccountManager;
        private readonly ITaskChangeCalculator _taskChangeCalculator;
        private readonly ITaskBusinessManager _taskBusinessManager;

        public TaskChangesProcessor(
            ILogger logger,
            IGoogleTaskAccountManager googleTaskAccountManager,
            ITaskChangeCalculator taskChangeCalculator,
            ITaskBusinessManager taskBusinessManager)
        {
            _logger = logger;
            _googleTaskAccountManager = googleTaskAccountManager;
            _taskChangeCalculator = taskChangeCalculator;
            _taskBusinessManager = taskBusinessManager;
        }

        public async Task ProcessTaskChangesAsync(TasksSynchronizerState tasksSynchronizerState)
        {
            var taskAccounts = await _googleTaskAccountManager.GetTaskAccountsAsync(tasksSynchronizerState);

            var taskChanges = new TaskChanges()
            {
                TasksToCreate = new List<Google::Task>(),
                TasksToModify = new List<Google::Task>(),
                TasksToClear = new List<Google::Task>()
            };

            foreach (var taskAccount in taskAccounts)
            {
                _taskChangeCalculator.CalculateTaskChanges(taskAccount, tasksSynchronizerState, taskChanges);
            }

            ProcessTasksToCreate(tasksSynchronizerState, taskAccounts, taskChanges.TasksToCreate);

            ProcessTasksToModify(tasksSynchronizerState, taskAccounts, taskChanges.TasksToModify);

            ProcessTasksToClear(tasksSynchronizerState, taskAccounts, taskChanges.TasksToClear);
        }

        private void ProcessTasksToClear(TasksSynchronizerState tasksSynchronizerState, List<TaskAccount> taskAccounts, List<Google::Task> tasksToClear)
        {
            if (tasksToClear.Any())
            {
                _logger.LogInformation("Tasks must be cleared.");

                foreach (var taskAccount in taskAccounts)
                {
                    _logger.LogInformation($"\tClearing Tasks for {taskAccount.SynchronizationTarget.GoogleAccountName}.");
                    var taskService = new TasksService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = taskAccount.UserCredential,
                        ApplicationName = "JSchafer Google Tasks Synchronizer",
                    });

                    taskService.Tasks.Clear(taskAccount.SynchronizationTarget.TaskListId).Execute();
                }

                foreach (var clearedTask in tasksToClear)
                {
                    var storedTask = _taskBusinessManager.GetStoredTaskById(clearedTask.Id, tasksSynchronizerState);

                    storedTask.Hidden = clearedTask.Hidden;
                }
            }
        }

        private void ProcessTasksToModify(TasksSynchronizerState tasksSynchronizerState, List<TaskAccount> taskAccounts, List<Google::Task> tasksToModify)
        {
            _logger.LogInformation($"{tasksToModify.Count} tasks to modify.");

            foreach (var modifiedTask in tasksToModify)
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

                    var taskIdsToModify = currentTasks.SelectMany(c => c.TaskIds).Where(c => c.AccountName == taskAccount.SynchronizationTarget.GoogleAccountName).Select(t => t.TaskId).ToList();

                    foreach (var taskId in taskIdsToModify)
                    {

                        var taskToModify = taskAccount.GoogleTasks?.FirstOrDefault(t => t.Id == taskId);

                        if (null == taskToModify)
                        {
                            taskToModify = taskService.Tasks.Get(taskAccount.SynchronizationTarget.TaskListId, taskId).Execute();
                        }

                        if (_taskBusinessManager.TasksAreLogicallyEqual(taskToModify, modifiedTask))
                        {
                            continue;
                        }

                        _logger.LogInformation($"\tModifying Task \"{taskToModify.Title}\" for {taskAccount.SynchronizationTarget.GoogleAccountName}.");

                        taskToModify.Title = modifiedTask.Title;
                        taskToModify.Notes = modifiedTask.Notes;
                        taskToModify.Due = modifiedTask.Due;
                        taskToModify.Status = modifiedTask.Status;
                        taskToModify.DueRaw = modifiedTask.DueRaw;
                        taskToModify.Deleted = modifiedTask.Deleted;
                        taskToModify.Completed = modifiedTask.Completed;
                        taskToModify.CompletedRaw = modifiedTask.CompletedRaw;

                        var updateRequest = taskService.Tasks.Update(taskToModify, taskAccount.SynchronizationTarget.TaskListId, taskToModify.Id);

                        updateRequest.Execute();
                    }
                }
            }
        }

        private void ProcessTasksToCreate(TasksSynchronizerState tasksSynchronizerState, List<TaskAccount> taskAccounts, List<Google::Task> tasksToCreate)
        {
            _logger.LogInformation($"{tasksToCreate.Count} tasks to create.");

            foreach (var createdTask in tasksToCreate)
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
                        _logger.LogInformation($"\tNew Task \"{createdTask.Title}\" for {taskAccount.SynchronizationTarget.GoogleAccountName}.");

                        var newTask = new Google::Task()
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

                        var insertRequest = taskService.Tasks.Insert(newTask, taskAccount.SynchronizationTarget.TaskListId);

                        var createdGoogleTask = insertRequest.Execute();

                        taskIdentifiers.Add(new TaskIdentifier() { AccountName = taskAccount.SynchronizationTarget.GoogleAccountName, TaskId = createdGoogleTask.Id });
                    }
                    else
                    {
                        taskIdentifiers.AddRange(tasks.Select(t => new TaskIdentifier() { AccountName = taskAccount.SynchronizationTarget.GoogleAccountName, TaskId = t.Id }));
                    }
                }

                tasksSynchronizerState.CurrentTasks.Add(new CurrentTask()
                {
                    Task = createdTask,
                    TaskIds = taskIdentifiers
                });
            }
        }
    }
}
