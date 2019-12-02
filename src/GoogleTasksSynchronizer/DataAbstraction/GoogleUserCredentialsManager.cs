using GoogleTasksSynchronizer.DataAbstraction.Models;
using Newtonsoft.Json;
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

        private async Task<GoogleUserCredentials> GetGoogleUserCredentials()
        {
            var applicationState = await _applicationStateManager.SelectAsync();

            return applicationState.GoogleUserCredentials;
        }

        private async Task UpdateGoogleUserCredentials(GoogleUserCredentials googleUserCredentials)
        {
            var applicationState = await _applicationStateManager.SelectAsync();

            applicationState.GoogleUserCredentials = googleUserCredentials;

            await _applicationStateManager.UpdateAsync(applicationState);
        }

        public async Task ClearAsync()
        {
            var googleUserCredentials = await GetGoogleUserCredentials();

            googleUserCredentials.Clear();

            await UpdateGoogleUserCredentials(googleUserCredentials);
        }

        public async Task DeleteAsync<T>(string key)
        {
            var googleUserCredentials = await GetGoogleUserCredentials();

            googleUserCredentials.Remove(key);

            await UpdateGoogleUserCredentials(googleUserCredentials);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var googleUserCredentials = await GetGoogleUserCredentials();

            if (!googleUserCredentials.ContainsKey(key))
            {
                return default;
            }

            var googleUserCredential = googleUserCredentials[key];

            return JsonConvert.DeserializeObject<T>(googleUserCredential);
        }

        public async Task StoreAsync<T>(string key, T value)
        {
            var googleUserCredentials = await GetGoogleUserCredentials();

            googleUserCredentials[key] = JsonConvert.SerializeObject(value);

            await UpdateGoogleUserCredentials(googleUserCredentials);
        }
    }
}
