using System.Threading.Tasks;
using GoogleTasksSynchronizer.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class TasksSynchronizerStateManager : ITasksSynchronizerStateManager
    {
        private readonly CloudBlockBlob _tasksSynchronizerStateBlob;

        public TasksSynchronizerStateManager(CloudBlockBlob tasksSynchronizerStateBlob)
        {
            _tasksSynchronizerStateBlob = tasksSynchronizerStateBlob;
        }

        public async Task<TasksSynchronizerState> SelectTasksSynchronizerStateAsync()
        {
            return null;
        }

        public async Task UpdateTasksSynchronizerStateAsync(TasksSynchronizerState tasksSynchronizerState)
        {
            string serializedState = "{\"test\":\"testValue\"}";
            await _tasksSynchronizerStateBlob.UploadTextAsync(serializedState);
        }
    }
}