using System;
using GoogleTasksSynchronizer.BusinessLogic;
using GoogleTasksSynchronizer.DataAbstraction;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer
{
    public class ProcessGoogleTaskChanges
    {
        private readonly ILogger _logger;
        private readonly ITasksSynchronizerStateManager _tasksSynchronizerStateManager;
        private readonly ITaskChangesProcessor _taskChangesProcessor;

        public ProcessGoogleTaskChanges(
            ILogger logger, 
            ITasksSynchronizerStateManager tasksSynchronizerStateManager,
            ITaskChangesProcessor taskChangesProcessor)
        {
            _logger = logger;
            _tasksSynchronizerStateManager = tasksSynchronizerStateManager;
            _taskChangesProcessor = taskChangesProcessor;
        }

        [FunctionName("ProcessGoogleTaskChanges")]
        public async Task Run(
            [TimerTrigger("*/15 * 6-23 * * *", RunOnStartup = true)]TimerInfo myTimer, 
            [Blob("jschaferfunctions/googleTasksSynchronizerState.json", Connection = "AzureWebJobsStorage")] CloudBlockBlob tasksSynchronizerStateBlob)
        {
            _logger.LogInformation($"ProcessGoogleTaskChanges Timer trigger function started at: {DateTime.Now}");

            var tasksSynchronizerState = await _tasksSynchronizerStateManager.SelectTasksSynchronizerStateAsync(tasksSynchronizerStateBlob);

            await _taskChangesProcessor.ProcessTaskChangesAsync(tasksSynchronizerState);

            await _tasksSynchronizerStateManager.UpdateTasksSynchronizerStateAsync(tasksSynchronizerState, tasksSynchronizerStateBlob);

            _logger.LogInformation($"ProcessGoogleTaskChanges Timer trigger function completed at: {DateTime.Now}");
        }
    }
}
