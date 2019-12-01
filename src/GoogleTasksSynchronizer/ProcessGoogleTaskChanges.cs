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
        private readonly IApplicationStateManager _applicationStateManager;
        private readonly ITaskChangesProcessor _taskChangesProcessor;

        public ProcessGoogleTaskChanges(
            ILogger logger, 
            IApplicationStateManager applicationStateManager,
            ITaskChangesProcessor taskChangesProcessor)
        {
            _logger = logger;
            _applicationStateManager = applicationStateManager;
            _taskChangesProcessor = taskChangesProcessor;
        }

        [FunctionName("ProcessGoogleTaskChanges")]
        public async Task Run(
            [TimerTrigger("*/15 * 6-23 * * *", RunOnStartup = true)]TimerInfo myTimer, 
            [Blob("jschaferfunctions/googleTasksSynchronizerState.json", Connection = "AzureWebJobsStorage")] CloudBlockBlob tasksSynchronizerStateBlob)
        {
            _logger.LogInformation($"ProcessGoogleTaskChanges Timer trigger function started at: {DateTime.Now}.  It {(myTimer.IsPastDue ? "was" : "was not")} past due.");

            await _applicationStateManager.InitializeFromBindingAsync(tasksSynchronizerStateBlob);

            await _taskChangesProcessor.ProcessTaskChangesAsync();

            _logger.LogInformation($"ProcessGoogleTaskChanges Timer trigger function completed at: {DateTime.Now}");
        }
    }
}
