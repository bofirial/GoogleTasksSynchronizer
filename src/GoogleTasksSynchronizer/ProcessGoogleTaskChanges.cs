using System;
using System.Collections;
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
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer
{
    public static class ProcessGoogleTaskChanges
    {
        [FunctionName("ProcessGoogleTaskChanges")]
        public static async System.Threading.Tasks.Task Run(
            [TimerTrigger("*/15 * 6-23 * * *")]TimerInfo myTimer, 
            [Blob("jschaferfunctions/googleTasksSynchronizerState.json", Connection = "AzureWebJobsStorage")] CloudBlockBlob googleTasksSynchronizerState, 
            TraceWriter log)
        {
            log.Info($"ProcessGoogleTaskChanges Timer trigger function started at: {DateTime.Now}");

            ITasksSynchronizerStateManager tasksSynchronizerStateManager = new TasksSynchronizerStateManager(googleTasksSynchronizerState, log);

            TasksSynchronizerState tasksSynchronizerState = await tasksSynchronizerStateManager.SelectTasksSynchronizerStateAsync();

            ITaskBusinessManager taskBusinessManager = new TaskBusinessManager(tasksSynchronizerState);

            IGoogleTaskAccountManager googleTaskAccountManager = new GoogleTaskAccountManager();

            List<TaskAccount> taskAccounts = googleTaskAccountManager.GetTaskAccounts(tasksSynchronizerState);

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

                        if (null == storedTask)
                        {
                            //New Task
                            log.Info("Found a New Task!");
                        }

                        if (!taskBusinessManager.TasksAreLogicallyEqual(task, storedTask))
                        {
                            //Modified Task
                            log.Info("Found a Modified Task!");
                        }
                    }
                }
            }


            log.Info($"ProcessGoogleTaskChanges Timer trigger function completed at: {DateTime.Now}");
        }
    }
}
