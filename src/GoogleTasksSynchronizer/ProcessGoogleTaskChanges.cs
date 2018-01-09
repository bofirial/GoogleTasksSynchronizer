using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
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

            IGoogleTaskAccountManager googleTaskAccountManager = new GoogleTaskAccountManager();

            List<TaskAccount> taskAccounts = googleTaskAccountManager.GetTaskAccounts(tasksSynchronizerState);



            //List<Task> createdTasks = new List<Task>();
            //List<Task> deletedTasks = new List<Task>();

            //foreach (var taskAccount in taskAccounts)
            //{
            //    TasksService taskService = new TasksService(new BaseClientService.Initializer()
            //    {
            //        HttpClientInitializer = taskAccount.UserCredential,
            //        ApplicationName = "JSchafer Google Tasks Synchronizer",
            //    });

            //    TasksResource.ListRequest listRequest = taskService.Tasks.List(taskAccount.TaskListId);

            //    taskAccount.GoogleTasks = listRequest.Execute().Items;

            //    log.Info($"{taskAccount.GoogleTasks.Count} tasks for {taskAccount.AccountName}");

            //    foreach (Task taskFromGoogle in taskAccount.GoogleTasks)
            //    {
            //        if (!tasksSynchronizerState.CurrentTasks.Any(t => taskBusinessManager.TasksAreLogicallyEqual(taskFromGoogle, t)))
            //        {
            //            createdTasks.Add(taskFromGoogle);
            //        }
            //    }

            //    foreach (Task currentTask in tasksSynchronizerState.CurrentTasks)
            //    {
            //        if (!taskAccount.GoogleTasks.Any(t => taskBusinessManager.TasksAreLogicallyEqual(currentTask, t)))
            //        {
            //            deletedTasks.Add(currentTask);
            //        }
            //    }
            //}


            log.Info($"ProcessGoogleTaskChanges Timer trigger function completed at: {DateTime.Now}");
        }
    }
}
