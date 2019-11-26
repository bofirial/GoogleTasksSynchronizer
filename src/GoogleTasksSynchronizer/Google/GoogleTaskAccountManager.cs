using System;
using System.Collections.Generic;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Tasks.v1;
using GoogleTasksSynchronizer.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer.Google
{
    public class GoogleTaskAccountManager : IGoogleTaskAccountManager
    {
        private readonly IOptions<TaskAccountsOptions> _taskAccountsOptions;

        public GoogleTaskAccountManager(IOptions<TaskAccountsOptions> taskAccountsOptions)
        {
            _taskAccountsOptions = taskAccountsOptions;
        }

        public async System.Threading.Tasks.Task<List<TaskAccount>> GetTaskAccountsAsync(TasksSynchronizerState tasksSynchronizerState)
        {
            var googleClientSecretProvider = new GoogleClientSecretProvider();

            var clientSecrets = googleClientSecretProvider.GetGoogleClientSecrets();

            //var taskAccounts =
            //    JsonConvert.DeserializeObject<List<TaskAccount>>(Environment.GetEnvironmentVariable("TaskAccounts"));
            var taskAccounts = _taskAccountsOptions.Value.TaskAccounts;

            foreach (var taskAccount in taskAccounts)
            {
                taskAccount.UserCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    new[] { TasksService.Scope.Tasks },
                    taskAccount.AccountName,
                    CancellationToken.None,
                    new TaskSynchronizerStateGoogleDataStore(tasksSynchronizerState));
            }

            return taskAccounts;
        }
    }
}