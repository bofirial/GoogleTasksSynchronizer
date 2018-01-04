using System;
using System.Collections.Generic;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Tasks.v1;
using GoogleTasksSynchronizer.Models;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer.Google
{
    public class GoogleTaskAccountManager : IGoogleTaskAccountManager
    {
        public List<TaskAccount> GetTaskAccounts(TasksSynchronizerState tasksSynchronizerState)
        {
            IGoogleClientSecretProvider googleClientSecretProvider = new GoogleClientSecretProvider();

            ClientSecrets clientSecrets = googleClientSecretProvider.GetGoogleClientSecrets();

            List<TaskAccount> taskAccounts =
                JsonConvert.DeserializeObject<List<TaskAccount>>(Environment.GetEnvironmentVariable("TaskAccounts"));

            foreach (var taskAccount in taskAccounts)
            {
                taskAccount.UserCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    new[] { TasksService.Scope.Tasks },
                    taskAccount.AccountName,
                    CancellationToken.None,
                    new TaskSynchronizerStateGoogleDataStore(tasksSynchronizerState)).Result;
            }

            return taskAccounts;
        }
    }
}