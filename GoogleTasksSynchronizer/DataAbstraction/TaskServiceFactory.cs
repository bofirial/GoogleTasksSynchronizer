using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using GoogleTasksSynchronizer.Configuration;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class TaskServiceFactory(
        IGoogleClientSecretProvider googleClientSecretProvider,
        IGoogleUserCredentialsManager googleUserCredentialsManager
            ) : ITaskServiceFactory
    {
        public async Task<TasksService> CreateTaskServiceAsync(SynchronizationTarget synchronizationTarget)
        {
            synchronizationTarget = synchronizationTarget ?? throw new ArgumentNullException(nameof(synchronizationTarget));

            return new TasksService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                                       await googleClientSecretProvider.GetGoogleClientSecrets(),
                                       [TasksService.Scope.Tasks],
                                       synchronizationTarget.GoogleAccountName,
                                       CancellationToken.None,
                                       googleUserCredentialsManager),
                ApplicationName = "JSchafer"
            });
        }
    }
}
