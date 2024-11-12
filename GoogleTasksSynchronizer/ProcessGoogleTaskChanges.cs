using Azure.Storage.Blobs;
using GoogleTasksSynchronizer.BusinessLogic;
using GoogleTasksSynchronizer.DataAbstraction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GoogleTasksSynchronizer
{
    public class ProcessGoogleTaskChanges(
        ILogger<ProcessGoogleTaskChanges> logger,
        IApplicationStateManager applicationStateManager,
        ITaskChangesProcessor taskChangesProcessor)
    {
        [Function("ProcessGoogleTaskChanges")]
        public async Task Run([TimerTrigger("*/15 * 6-23 * * *", RunOnStartup = true)] TimerInfo myTimer,
            [BlobInput("jschaferfunctions/tasksSynchronizerStateBlob.json", Connection = "AzureWebJobsStorage")] BlobClient tasksSynchronizerStateBlob)
        {
            myTimer = myTimer ?? throw new ArgumentNullException(nameof(myTimer));

            logger.LogInformation($"ProcessGoogleTaskChanges Timer trigger function started at: {DateTime.Now}.  It {(myTimer.IsPastDue ? "was" : "was not")} past due.");

            await applicationStateManager.InitializeFromBindingAsync(tasksSynchronizerStateBlob);

            await taskChangesProcessor.ProcessTaskChangesAsync();

            logger.LogInformation($"ProcessGoogleTaskChanges Timer trigger function completed at: {DateTime.Now}");
        }
    }
}
