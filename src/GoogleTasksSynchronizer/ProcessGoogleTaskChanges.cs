using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.BusinessLogic;
using GoogleTasksSynchronizer.DataAbstraction;
using GoogleTasksSynchronizer.Google;
using GoogleTasksSynchronizer.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer
{
    public static class ProcessGoogleTaskChanges
    {
        [FunctionName("ProcessGoogleTaskChanges")]
        public static async System.Threading.Tasks.Task Run(
            [TimerTrigger("*/15 * 6-23 * * *", RunOnStartup = true)]TimerInfo myTimer, 
            [Blob("jschaferfunctions/googleTasksSynchronizerState.json", Connection = "AzureWebJobsStorage")] CloudBlockBlob googleTasksSynchronizerState, 
            TraceWriter log)
        {
            log.Info($"ProcessGoogleTaskChanges Timer trigger function started at: {DateTime.Now}");

            ITasksSynchronizerStateManager tasksSynchronizerStateManager = new TasksSynchronizerStateManager(googleTasksSynchronizerState, log);

            TasksSynchronizerState tasksSynchronizerState = await tasksSynchronizerStateManager.SelectTasksSynchronizerStateAsync();

            ITaskBusinessManager taskBusinessManager = new TaskBusinessManager(tasksSynchronizerState);

            IGoogleTaskAccountManager googleTaskAccountManager = new GoogleTaskAccountManager();

            List<TaskAccount> taskAccounts = googleTaskAccountManager.GetTaskAccounts(tasksSynchronizerState);

            List<Task> createdTasks = new List<Task>();
            List<Task> modifiedTasks = new List<Task>();
            List<Task> clearedTasks = new List<Task>();

            foreach (var taskAccount in taskAccounts)
            {
                TasksService taskService = new TasksService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = taskAccount.UserCredential,
                    ApplicationName = "JSchafer Google Tasks Synchronizer",
                });

                TasksResource.ListRequest listRequest = taskService.Tasks.List(taskAccount.TaskListId);

                listRequest.ShowCompleted = true;
                listRequest.ShowDeleted = true;
                listRequest.ShowHidden = true;
                listRequest.UpdatedMin = DateTime.Today.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

                taskAccount.GoogleTasks = listRequest.Execute().Items;

                log.Info($"{taskAccount.GoogleTasks?.Count ?? 0} updated tasks today for {taskAccount.AccountName}");

                if (taskAccount.GoogleTasks != null)
                {
                    foreach (Task task in taskAccount.GoogleTasks)
                    {
                        Task storedTask = taskBusinessManager.GetStoredTaskById(task.Id);

                        if (storedTask != null && taskBusinessManager.TasksMustBeCleared(task, storedTask))
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

                        if (!taskBusinessManager.TasksAreLogicallyEqual(task, storedTask))
                        {
                            modifiedTasks.Add(task);
                            continue;
                        }
                    }
                }
            }

            log.Info($"{createdTasks.Count} tasks to create.");

            foreach (var createdTask in createdTasks)
            {
                List<TaskIdentifier> taskIdentifiers = new List<TaskIdentifier>();

                foreach (TaskAccount taskAccount in taskAccounts)
                {
                    TasksService taskService = new TasksService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = taskAccount.UserCredential,
                        ApplicationName = "JSchafer Google Tasks Synchronizer",
                    });

                    
                    Task[] tasks = taskAccount.GoogleTasks?.Where(t => taskBusinessManager.TasksAreLogicallyEqual(createdTask, t)).ToArray();

                    if (null == tasks || !tasks.Any())
                    {
                        log.Info($"\tNew Task \"{createdTask.Title}\" for {taskAccount.AccountName}.");

                        Task newTask = new Task()
                        {
                            Title = createdTask.Title,
                            Notes = createdTask.Notes,
                            Due = createdTask.Due,
                            Status = createdTask.Status,
                            DueRaw = createdTask.DueRaw,
                            Deleted = createdTask.Deleted,
                            Hidden = createdTask.Hidden,
                            Completed = createdTask.Completed,
                            CompletedRaw = createdTask.CompletedRaw,
                        };

                        TasksResource.InsertRequest insertRequest = taskService.Tasks.Insert(newTask, taskAccount.TaskListId);

                        Task createdGoogleTask = insertRequest.Execute();

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

            log.Info($"{modifiedTasks.Count} tasks to modify.");

            foreach (var modifiedTask in modifiedTasks)
            {
                List<CurrentTask> currentTasks = tasksSynchronizerState.CurrentTasks.Where(c => c.TaskIds.Any(t => modifiedTask.Id == t.TaskId)).ToList();

                foreach (CurrentTask currentTask in currentTasks)
                {
                    currentTask.Task = modifiedTask;
                }

                foreach (TaskAccount taskAccount in taskAccounts)
                {
                    TasksService taskService = new TasksService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = taskAccount.UserCredential,
                        ApplicationName = "JSchafer Google Tasks Synchronizer",
                    });

                    List<string> taskIdsToModify = currentTasks.SelectMany(c => c.TaskIds).Where(c => c.AccountName == taskAccount.AccountName).Select(t => t.TaskId).ToList();

                    foreach (string taskId in taskIdsToModify)
                    {

                        Task taskToModify = taskAccount.GoogleTasks?.FirstOrDefault(t => t.Id == taskId);

                        if (null == taskToModify)
                        {
                            taskToModify = taskService.Tasks.Get(taskAccount.TaskListId, taskId).Execute();
                        }

                        if (taskBusinessManager.TasksAreLogicallyEqual(taskToModify, modifiedTask))
                        {
                            continue;
                        }
                        
                        log.Info($"\tModifying Task \"{taskToModify.Title}\" for {taskAccount.AccountName}.");

                        taskToModify.Title = modifiedTask.Title;
                        taskToModify.Notes = modifiedTask.Notes;
                        taskToModify.Due = modifiedTask.Due;
                        taskToModify.Status = modifiedTask.Status;
                        taskToModify.DueRaw = modifiedTask.DueRaw;
                        taskToModify.Deleted = modifiedTask.Deleted;
                        taskToModify.Completed = modifiedTask.Completed;
                        taskToModify.CompletedRaw = modifiedTask.CompletedRaw;

                        TasksResource.UpdateRequest updateRequest = taskService.Tasks.Update(taskToModify, taskAccount.TaskListId, taskToModify.Id);

                        updateRequest.Execute();
                    }
                }
            }

            if (clearedTasks.Any())
            {
                log.Info("Tasks must be cleared.");

                foreach (var taskAccount in taskAccounts)
                {
                    log.Info($"\tClearing Tasks for {taskAccount.AccountName}.");
                    TasksService taskService = new TasksService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = taskAccount.UserCredential,
                        ApplicationName = "JSchafer Google Tasks Synchronizer",
                    });

                    taskService.Tasks.Clear(taskAccount.TaskListId).Execute();
                }

                foreach (var clearedTask in clearedTasks)
                {
                    var storedTask = taskBusinessManager.GetStoredTaskById(clearedTask.Id);

                    storedTask.Hidden = clearedTask.Hidden;
                }
            }

            tasksSynchronizerState.CurrentTasks.RemoveAll(c =>
                c.Task.Updated < DateTime.Today.AddDays(-7) && (c.Task.Hidden == true || c.Task.Deleted == true));

            await tasksSynchronizerStateManager.UpdateTasksSynchronizerStateAsync(tasksSynchronizerState);

            log.Info($"ProcessGoogleTaskChanges Timer trigger function completed at: {DateTime.Now}");
        }
    }
}
