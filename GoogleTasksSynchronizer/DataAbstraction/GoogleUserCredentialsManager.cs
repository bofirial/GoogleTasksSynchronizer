using GoogleTasksSynchronizer.DataAbstraction.Models;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class GoogleUserCredentialsManager(IApplicationStateManager applicationStateManager) : IGoogleUserCredentialsManager
    {
        private async Task<GoogleUserCredentialsDictionary> GetGoogleUserCredentials()
        {
            var applicationState = await applicationStateManager.SelectAsync();

            return applicationState.GoogleUserCredentials;
        }

        private async Task UpdateGoogleUserCredentials(GoogleUserCredentialsDictionary googleUserCredentials)
        {
            var applicationState = await applicationStateManager.SelectAsync();

            applicationState.GoogleUserCredentials = googleUserCredentials;

            await applicationStateManager.UpdateAsync(applicationState);
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

            if (!googleUserCredentials.TryGetValue(key, out var value))
            {
                return default;
            }

            var googleUserCredential = value;

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
