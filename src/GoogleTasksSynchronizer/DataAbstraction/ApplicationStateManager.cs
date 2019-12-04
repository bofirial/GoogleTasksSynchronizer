using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class ApplicationStateManager : IApplicationStateManager
    {
        private readonly ILogger<ApplicationStateManager> _logger;

        private CloudBlockBlob _applicationStateBlob;
        private ApplicationState _applicationState;

        private bool _initialized = false;

        public ApplicationStateManager(ILogger<ApplicationStateManager> logger)
        {
            _logger = logger;
        }

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
