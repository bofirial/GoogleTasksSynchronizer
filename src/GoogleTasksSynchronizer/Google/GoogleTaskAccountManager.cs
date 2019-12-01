//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Tasks.v1;
//using GoogleTasksSynchronizer.Configuration;
//using GoogleTasksSynchronizer.Models;
//using Microsoft.Extensions.Options;

//namespace GoogleTasksSynchronizer.Google
//{
//    public class GoogleTaskAccountManager : IGoogleTaskAccountManager
//    {
//        private readonly IOptions<SynchronizationTargetsOptions> _taskAccountsOptions;

//        public GoogleTaskAccountManager(IOptions<SynchronizationTargetsOptions> taskAccountsOptions)
//        {
//            _taskAccountsOptions = taskAccountsOptions;
//        }

//        public async Task<List<TaskAccount>> GetTaskAccountsAsync(TasksSynchronizerState tasksSynchronizerState)
//        {
//            var googleClientSecretProvider = new GoogleClientSecretProvider();

//            var clientSecrets = googleClientSecretProvider.GetGoogleClientSecrets();

//            //var taskAccounts =
//            //    JsonConvert.DeserializeObject<List<TaskAccount>>(Environment.GetEnvironmentVariable("TaskAccounts"));

//            var taskAccountStateTasks = _taskAccountsOptions.Value.SynchronizationTargets.Select(async st =>
//               new TaskAccount()
//               {
//                   SynchronizationTarget = st,
//                   UserCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
//                       clientSecrets,
//                       new[] { TasksService.Scope.Tasks },
//                       st.GoogleAccountName,
//                       CancellationToken.None,
//                       new TaskSynchronizerStateGoogleDataStore(tasksSynchronizerState))
//               });

//            return (await Task.WhenAll(taskAccountStateTasks)).ToList();
//        }
//    }
//}