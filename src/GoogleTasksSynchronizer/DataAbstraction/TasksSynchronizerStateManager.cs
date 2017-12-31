using System;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.Models;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class TasksSynchronizerStateManager : ITasksSynchronizerStateManager
    {
        private readonly CloudBlockBlob _tasksSynchronizerStateBlob;
        private readonly TraceWriter _log;

        public TasksSynchronizerStateManager(CloudBlockBlob tasksSynchronizerStateBlob, TraceWriter log)
        {
            _tasksSynchronizerStateBlob = tasksSynchronizerStateBlob;
            _log = log;
        }

        public async Task<TasksSynchronizerState> SelectTasksSynchronizerStateAsync()
        {
            string rawData = await _tasksSynchronizerStateBlob.DownloadTextAsync();

            try
            {
                return JsonConvert.DeserializeObject<TasksSynchronizerState>(rawData);
            }
            catch (Exception e)
            {
                _log.Warning($"Failed to Deserialize the TasksSynchronizerState: {rawData}.  Exception Details: {e.Message}");

                return new TasksSynchronizerState();
            }
        }

        public Task UpdateTasksSynchronizerStateAsync(TasksSynchronizerState tasksSynchronizerState)
        {
            string serializedTasksSynchronizerState = JsonConvert.SerializeObject(tasksSynchronizerState);

            return _tasksSynchronizerStateBlob.UploadTextAsync(serializedTasksSynchronizerState);
        }
    }
}