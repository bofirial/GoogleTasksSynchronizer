using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Util.Store;
using GoogleTasksSynchronizer.Models;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer.Google
{
    public class TaskSynchronizerStateGoogleDataStore : IDataStore
    {
        private readonly TasksSynchronizerState _tasksSynchronizerState;

        public TaskSynchronizerStateGoogleDataStore(TasksSynchronizerState tasksSynchronizerState)
        {
            _tasksSynchronizerState = tasksSynchronizerState;
        }

        public Task StoreAsync<T>(string key, T value)
        {
            _tasksSynchronizerState.GoogleUserCredentials[key] = JsonConvert.SerializeObject(value);

            return Task.CompletedTask;
        }

        public Task DeleteAsync<T>(string key)
        {
            _tasksSynchronizerState.GoogleUserCredentials.Remove(key);

            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (!_tasksSynchronizerState.GoogleUserCredentials.ContainsKey(key))
            {
                return Task.FromResult(default(T));
            }

            var googleUserCredential = _tasksSynchronizerState.GoogleUserCredentials[key];

            return Task.FromResult(JsonConvert.DeserializeObject<T>(googleUserCredential));
        }

        public Task ClearAsync()
        {
            _tasksSynchronizerState.GoogleUserCredentials.Clear();

            return Task.CompletedTask;
        }
    }
}
