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
            //log.Info($"SyncGoogleTasks Timer trigger function started at: {DateTime.Now}");

            //ITasksSynchronizerStateManager tasksSynchronizerStateManager = new TasksSynchronizerStateManager(googleTasksSynchronizerState, log);

            //TasksSynchronizerState tasksSynchronizerState = await tasksSynchronizerStateManager.SelectTasksSynchronizerStateAsync();

            //IGoogleTaskAccountManager googleTaskAccountManager = new GoogleTaskAccountManager();

            //List<TaskAccount> taskAccounts = googleTaskAccountManager.GetTaskAccounts(tasksSynchronizerState);

            ////DO STUFF

            

            //foreach (var taskAccount in taskAccounts)
            //{
            //    TasksService taskService = new TasksService(new BaseClientService.Initializer()
            //    {
            //        HttpClientInitializer = taskAccount.UserCredential,
            //        ApplicationName = "JSchafer Google Tasks Synchronizer",
            //    });

            //    TasksResource.ListRequest listRequest = taskService.Tasks.List(taskAccount.TaskListId);

            //    listRequest.ShowDeleted = true;
            //    listRequest.ShowCompleted = true;
            //    listRequest.ShowHidden = true;
            //    //listRequest.UpdatedMin = tasksSynchronizerState.LastQueryTime.ToString();

            //    var execute = listRequest.Execute();
            //    IList<Task> tasks = execute.Items;

            //    break;
            //}



            ////END DO STUFF

            //////await tasksSynchronizerStateManager.UpdateTasksSynchronizerStateAsync(tasksSynchronizerState);

            //////log.Info($"SyncGoogleTasks Timer trigger function completed at: {DateTime.Now}");
            

            //////static string ApplicationName = "Google Tasks API Quickstart";

            //////// Create Google Tasks API service.
            //////var service = new TasksService(new BaseClientService.Initializer()
            //////{
            //////    HttpClientInitializer = credential,
            //////    ApplicationName = ApplicationName,
            //////});

            //////// Define parameters of request.
            //////TasklistsResource.ListRequest listRequest = service.Tasklists.List();
            //////listRequest.MaxResults = 10;

            //////// List task lists.
            //////IList<TaskList> taskLists = listRequest.Execute().Items;
            //////log.Info("Task Lists:");
            //////if (taskLists != null && taskLists.Count > 0)
            //////{
            //////    foreach (var taskList in taskLists)
            //////    {
            //////        log.Info($"{taskList.Title} ({taskList.Id})");
            //////    }
            //////}
            //////else
            //////{
            //////    log.Info("No task lists found.");
            //////}
        }
    }
}
