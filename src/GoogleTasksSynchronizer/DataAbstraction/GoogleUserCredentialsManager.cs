using GoogleTasksSynchronizer.DataAbstraction.Interfaces;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class GoogleUserCredentialsManager : IGoogleUserCredentialsManager
    {
        private readonly IApplicationStateManager _applicationStateManager;

        public GoogleUserCredentialsManager(IApplicationStateManager applicationStateManager)
        {
            _applicationStateManager = applicationStateManager;
        }

        public async Task<GoogleUserCredentials> SelectAsync()
        {
            return (await _applicationStateManager.SelectAsync()).GoogleUserCredentials;
        }

        public async Task UpdateAsync(GoogleUserCredentials googleUserCredentials)
        {
            var applicationState = await _applicationStateManager.SelectAsync();

            applicationState.GoogleUserCredentials = googleUserCredentials;

            await _applicationStateManager.UpdateAsync(applicationState);
        }
    }
}
