using System;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class ApplicationStateManager : IApplicationStateManager
    {
        private CloudBlockBlob _applicationStateBlob;
        private ApplicationState _applicationState;

        private bool _initialized = false;

        public Task InitializeFromBindingAsync(CloudBlockBlob applicationStateBlob)
        {
            _applicationStateBlob = applicationStateBlob;

            _initialized = true;

            return Task.CompletedTask;
        }

        public async Task<ApplicationState> SelectAsync()
        {
            ValidateInitilization();

            if (_applicationState == null)
            {
                var rawData = await _applicationStateBlob.DownloadTextAsync();

                return JsonConvert.DeserializeObject<ApplicationState>(rawData);
            }

            return _applicationState;
        }

        public Task UpdateAsync(ApplicationState applicationState)
        {
            ValidateInitilization();

            var serializedTasksSynchronizerState = JsonConvert.SerializeObject(applicationState);

            _applicationState = applicationState;

            return _applicationStateBlob.UploadTextAsync(serializedTasksSynchronizerState);
        }

        private void ValidateInitilization()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("ApplicationStateManager must be initialized with a storage binding before it can be utilized");
            }
        }
    }
}
