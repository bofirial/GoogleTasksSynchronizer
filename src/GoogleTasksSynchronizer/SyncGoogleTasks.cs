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
            TraceWriter log)
        {
            log.Info($"SyncGoogleTasks Timer trigger function started at: {DateTime.Now}");

            ITasksSynchronizerStateManager tasksSynchronizerStateManager = new TasksSynchronizerStateManager(googleTasksSynchronizerState, log);

            TasksSynchronizerState tasksSynchronizerState = await tasksSynchronizerStateManager.SelectTasksSynchronizerStateAsync();

            IGoogleTaskAccountManager googleTaskAccountManager = new GoogleTaskAccountManager();

            List<TaskAccount> taskAccounts = googleTaskAccountManager.GetTaskAccounts(tasksSynchronizerState);

            ITaskBusinessManager taskBusinessManager = new TaskBusinessManager(tasksSynchronizerState);

            List<Task> createdTasks = new List<Task>();
            List<Task> deletedTasks = new List<Task>();

            foreach (var taskAccount in taskAccounts)
            {
                TasksService taskService = new TasksService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = taskAccount.UserCredential,
                    ApplicationName = "JSchafer Google Tasks Synchronizer",
                });

                TasksResource.ListRequest listRequest = taskService.Tasks.List(taskAccount.TaskListId);
                
                taskAccount.GoogleTasks = new List<Task>();

                Tasks taskResult = null;

                do
                {
                    listRequest.PageToken = taskResult?.NextPageToken;

                    taskResult = listRequest.Execute();

                    if (null != taskResult?.Items)
                    {
                        taskAccount.GoogleTasks.AddRange(taskResult.Items); 
                    }

                } while (taskResult?.NextPageToken != null);

                log.Info($"{taskAccount.GoogleTasks.Count} tasks for {taskAccount.AccountName}");

                foreach (Task taskFromGoogle in taskAccount.GoogleTasks)
                {
                    if (!tasksSynchronizerState.CurrentTasks.Any(c => taskBusinessManager.TasksAreLogicallyEqual(taskFromGoogle, c.Task)))
                    {
                        createdTasks.Add(taskFromGoogle);
                    }
                }

                foreach (CurrentTask currentTask in tasksSynchronizerState.CurrentTasks)
                {
                    if (!taskAccount.GoogleTasks.Any(t => taskBusinessManager.TasksAreLogicallyEqual(currentTask.Task, t)))
                    {
                        deletedTasks.Add(currentTask.Task);
                    }
                }
            }

            log.Info($"{createdTasks.Count} tasks to create.");

            foreach (var createdTask in createdTasks)
            {
                List<TaskIdentifier> taskIdentifiers = new List<TaskIdentifier>();

                foreach (TaskAccount taskAccount in taskAccounts)
                {
                    Task[] tasks = taskAccount.GoogleTasks.Where(t => taskBusinessManager.TasksAreLogicallyEqual(createdTask, t)).ToArray();

                    if (!tasks.Any())
                    {
                        log.Info($"\tNew Task \"{createdTask.Title}\" for {taskAccount.AccountName}.");

                        TasksService taskService = new TasksService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = taskAccount.UserCredential,
                            ApplicationName = "JSchafer Google Tasks Synchronizer",
                        });

                        Task newTask = new Task()
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

                        TasksResource.InsertRequest insertRequest = taskService.Tasks.Insert(newTask, taskAccount.TaskListId);

                        Task createdGoogleTask = insertRequest.Execute();

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

            log.Info($"{deletedTasks.Count} tasks to delete.");

            foreach (var deletedTask in deletedTasks)
            {
                tasksSynchronizerState.CurrentTasks.RemoveAll(c =>
                    taskBusinessManager.TasksAreLogicallyEqual(deletedTask, c.Task));

                foreach (TaskAccount taskAccount in taskAccounts)
                {
                    IEnumerable<Task> googleTasksToDelete = taskAccount.GoogleTasks.Where(t => taskBusinessManager.TasksAreLogicallyEqual(deletedTask, t));

                    foreach (Task taskToDelete in googleTasksToDelete)
                    {
                        log.Info($"\tDeleting Task \"{taskToDelete.Title}\" for {taskAccount.AccountName}.");

                        TasksService taskService = new TasksService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = taskAccount.UserCredential,
                            ApplicationName = "JSchafer Google Tasks Synchronizer",
                        });

                        TasksResource.DeleteRequest deleteRequest = taskService.Tasks.Delete(taskAccount.TaskListId, taskToDelete.Id);

                        deleteRequest.Execute();
                    }
                }
            }

            await tasksSynchronizerStateManager.UpdateTasksSynchronizerStateAsync(tasksSynchronizerState);

            log.Info($"SyncGoogleTasks Timer trigger function completed at: {DateTime.Now}");

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
