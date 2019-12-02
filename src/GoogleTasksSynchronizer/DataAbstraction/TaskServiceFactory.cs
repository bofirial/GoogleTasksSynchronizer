using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using GoogleTasksSynchronizer.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class TaskServiceFactory : ITaskServiceFactory
    {
        private readonly IGoogleClientSecretProvider _googleClientSecretProvider;
        private readonly IGoogleUserCredentialsManager _googleUserCredentialsManager;

        public TaskServiceFactory(
            IGoogleClientSecretProvider googleClientSecretProvider,
            IGoogleUserCredentialsManager googleUserCredentialsManager
            )
        {
            _googleClientSecretProvider = googleClientSecretProvider;
            _googleUserCredentialsManager = googleUserCredentialsManager;
        }

        public async Task<TasksService> CreateTaskServiceAsync(SynchronizationTarget synchronizationTarget)
        {
            return new TasksService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                                       _googleClientSecretProvider.GetGoogleClientSecrets(),
                                       new[] { TasksService.Scope.Tasks },
                                       synchronizationTarget.GoogleAccountName,
                                       CancellationToken.None,
                                       _googleUserCredentialsManager),
                ApplicationName = "JSchafer"
            });
        }
    }
}
