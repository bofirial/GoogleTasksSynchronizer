//using System;
//using System.Threading.Tasks;
//using GoogleTasksSynchronizer.Models;
//using Microsoft.Azure.WebJobs.Host;
//using Microsoft.Extensions.Logging;
//using Microsoft.WindowsAzure.Storage.Blob;
//using Newtonsoft.Json;

//namespace GoogleTasksSynchronizer.DataAbstraction
//{
//    public class TasksSynchronizerStateManager : ITasksSynchronizerStateManager
//    {
//        private readonly ILogger _logger;

//        public TasksSynchronizerStateManager(ILogger logger)
//        {
//            _logger = logger;
//        }

//        public async Task<TasksSynchronizerState> SelectTasksSynchronizerStateAsync(CloudBlockBlob tasksSynchronizerStateBlob)
//        {
//            var rawData = await tasksSynchronizerStateBlob.DownloadTextAsync();

//            try
//            {
//                return JsonConvert.DeserializeObject<TasksSynchronizerState>(rawData);
//            }
//            catch (Exception e)
//            {
//                _logger.LogWarning($"Failed to Deserialize the TasksSynchronizerState: {rawData}.  Exception Details: {e.Message}");

//                return new TasksSynchronizerState();
//            }
//        }

//        public Task UpdateTasksSynchronizerStateAsync(TasksSynchronizerState tasksSynchronizerState, CloudBlockBlob tasksSynchronizerStateBlob)
//        {
//            tasksSynchronizerState.CurrentTasks.RemoveAll(c =>
//                c.Task.Updated < DateTime.Today.AddDays(-7) && (c.Task.Hidden == true || c.Task.Deleted == true));

//            var serializedTasksSynchronizerState = JsonConvert.SerializeObject(tasksSynchronizerState);

//            return tasksSynchronizerStateBlob.UploadTextAsync(serializedTasksSynchronizerState);
//        }
//    }
//}