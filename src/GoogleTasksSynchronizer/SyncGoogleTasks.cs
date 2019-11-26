using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.BusinessLogic;
using GoogleTasksSynchronizer.DataAbstraction;
using GoogleTasksSynchronizer.Google;
using GoogleTasksSynchronizer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer
{
    public static class SyncGoogleTasks
    {
        [FunctionName("SyncGoogleTasks")]
        public static async System.Threading.Tasks.Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req,
            [Blob("jschaferfunctions/googleTasksSynchronizerState.json", Connection = "AzureWebJobsStorage")] CloudBlockBlob googleTasksSynchronizerState,
            ILogger log)
        {
            log.LogInformation($"SyncGoogleTasks Timer trigger function started at: {DateTime.Now}");

            var tasksSynchronizerStateManager = new TasksSynchronizerStateManager(googleTasksSynchronizerState, log);

            var tasksSynchronizerState = await tasksSynchronizerStateManager.SelectTasksSynchronizerStateAsync();

            var taskBusinessManager = new TaskBusinessManager(tasksSynchronizerState);

            var googleTaskAccountManager = new GoogleTaskAccountManager();

            var taskAccounts = googleTaskAccountManager.GetTaskAccounts(tasksSynchronizerState);

            var createdTasks = new List<Task>();
            var deletedTasks = new List<Task>();

            foreach (var taskAccount in taskAccounts)
            {
                var taskService = new TasksService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = taskAccount.UserCredential,
                    ApplicationName = "JSchafer Google Tasks Synchronizer",
                });

                var listRequest = taskService.Tasks.List(taskAccount.TaskListId);
                
                taskAccount.GoogleTasks = taskBusinessManager.RequestAllGoogleTasks(listRequest);

                log.LogInformation($"{taskAccount.GoogleTasks.Count} tasks for {taskAccount.AccountName}");

                foreach (var task in taskAccount.GoogleTasks)
                {
                    if (!tasksSynchronizerState.CurrentTasks.Any(c => taskBusinessManager.TasksAreLogicallyEqual(task, c.Task)))
                    {
                        createdTasks.Add(task);
                    }
                }

                foreach (var currentTask in tasksSynchronizerState.CurrentTasks)
                {
                    if (!taskAccount.GoogleTasks.Any(t => taskBusinessManager.TasksAreLogicallyEqual(currentTask.Task, t)))
                    {
                        deletedTasks.Add(currentTask.Task);
                    }
                }
            }

            log.LogInformation($"{createdTasks.Count} tasks to create.");

            foreach (var createdTask in createdTasks)
            {
                var taskIdentifiers = new List<TaskIdentifier>();

                foreach (var taskAccount in taskAccounts)
                {
                    var tasks = taskAccount.GoogleTasks.Where(t => taskBusinessManager.TasksAreLogicallyEqual(createdTask, t)).ToArray();

                    if (!tasks.Any())
                    {
                        log.LogInformation($"\tNew Task \"{createdTask.Title}\" for {taskAccount.AccountName}.");

                        var taskService = new TasksService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = taskAccount.UserCredential,
                            ApplicationName = "JSchafer Google Tasks Synchronizer",
                        });

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

                        taskIdentifiers.Add(new TaskIdentifier() {AccountName = taskAccount.AccountName, TaskId = createdGoogleTask.Id});
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

            log.LogInformation($"{deletedTasks.Count} tasks to delete.");

            foreach (var deletedTask in deletedTasks)
            {
                tasksSynchronizerState.CurrentTasks.RemoveAll(c =>
                    taskBusinessManager.TasksAreLogicallyEqual(deletedTask, c.Task));

                foreach (var taskAccount in taskAccounts)
                {
                    var googleTasksToDelete = taskAccount.GoogleTasks.Where(t => taskBusinessManager.TasksAreLogicallyEqual(deletedTask, t));

                    foreach (var taskToDelete in googleTasksToDelete)
                    {
                        log.LogInformation($"\tDeleting Task \"{taskToDelete.Title}\" for {taskAccount.AccountName}.");

                        var taskService = new TasksService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = taskAccount.UserCredential,
                            ApplicationName = "JSchafer Google Tasks Synchronizer",
                        });

                        var deleteRequest = taskService.Tasks.Delete(taskAccount.TaskListId, taskToDelete.Id);

                        deleteRequest.Execute();
                    }
                }
            }

            await tasksSynchronizerStateManager.UpdateTasksSynchronizerStateAsync(tasksSynchronizerState);

            log.LogInformation($"SyncGoogleTasks Timer trigger function completed at: {DateTime.Now}");

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
