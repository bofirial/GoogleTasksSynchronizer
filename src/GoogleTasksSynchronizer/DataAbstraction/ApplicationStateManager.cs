using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class ApplicationStateManager : IApplicationStateManager
    {
        private readonly ILogger _logger;

        private CloudBlockBlob _applicationStateBlob;
        private ApplicationState _applicationState;

        public ApplicationStateManager(ILogger logger)
        {
            _logger = logger;
        }

        public Task InitializeFromBindingAsync(CloudBlockBlob applicationStateBlob)
        {
            _applicationStateBlob = applicationStateBlob;

            return Task.CompletedTask;
        }

        public async Task<ApplicationState> SelectAsync()
        {
            if (_applicationState == null)
            {
                var rawData = await _applicationStateBlob.DownloadTextAsync();

                try
                {
                    return JsonConvert.DeserializeObject<ApplicationState>(rawData);
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"Failed to Deserialize the ApplicationState: {rawData}.  Exception Details: {e.Message}");

                    _applicationState = new ApplicationState();
                }
            }

            return _applicationState;
        }

        public Task UpdateAsync(ApplicationState applicationState)
        {
            var serializedTasksSynchronizerState = JsonConvert.SerializeObject(applicationState);

            _applicationState = applicationState;

            return _applicationStateBlob.UploadTextAsync(serializedTasksSynchronizerState);
        }
    }
}
