using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class ApplicationStateManager : IApplicationStateManager
    {
        private BlobClient _applicationStateBlob;
        private ApplicationState _applicationState;

        private bool _initialized = false;

        public Task InitializeFromBindingAsync(BlobClient applicationStateBlob)
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
                var blobDownloadResult = await _applicationStateBlob.DownloadContentAsync();

                var rawData = blobDownloadResult.Value.Content.ToString();

                return JsonConvert.DeserializeObject<ApplicationState>(rawData);
            }

            return _applicationState;
        }

        public Task UpdateAsync(ApplicationState applicationState)
        {
            ValidateInitilization();

            var serializedTasksSynchronizerState = JsonConvert.SerializeObject(applicationState);

            _applicationState = applicationState;

            return _applicationStateBlob.UploadAsync(BinaryData.FromString(serializedTasksSynchronizerState), overwrite: true);
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
