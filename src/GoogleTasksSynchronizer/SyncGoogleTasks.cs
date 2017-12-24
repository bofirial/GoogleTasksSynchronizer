using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util.Store;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;


namespace GoogleTasksSyncronizer
{
    public static class SyncGoogleTasks
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/tasks-dotnet-quickstart.json
        static string[] Scopes = { TasksService.Scope.TasksReadonly };
        static string ApplicationName = "Google Tasks API Quickstart";

        [FunctionName("SyncGoogleTasks")]
        public static void Run([TimerTrigger("*/5 * 6-23 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            UserCredential credential;

            string googleClientSecretJson = Environment.GetEnvironmentVariable("GoogleClientSecret");

            Stream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(googleClientSecretJson));

            GoogleClientSecrets googleClientSecrets = new GoogleClientSecrets();

            string credPath = System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/tasks-dotnet-quickstart.json");

            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(memoryStream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            log.Info("Credential file saved to: " + credPath);

            // Create Google Tasks API service.
            var service = new TasksService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            TasklistsResource.ListRequest listRequest = service.Tasklists.List();
            listRequest.MaxResults = 10;

            // List task lists.
            IList<TaskList> taskLists = listRequest.Execute().Items;
            log.Info("Task Lists:");
            if (taskLists != null && taskLists.Count > 0)
            {
                foreach (var taskList in taskLists)
                {
                    log.Info($"{taskList.Title} ({taskList.Id})");
                }
            }
            else
            {
                log.Info("No task lists found.");
            }

        }
    }
}
