using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace GoogleTasksSynchronizer
{
    public class LogAvailableTaskListIds(ISynchronizationTargetsProvider synchronizationTargetManager, ITaskListLogger taskListLogger)
    {
        [Function("LogAvailableTaskListIds")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));

            string[] googleAccountNames = request.Query["googleAccountName"];

            if (null == googleAccountNames || googleAccountNames.Length == 0)
            {
                var synchronizationTargets = await synchronizationTargetManager.GetAsync();

                googleAccountNames = synchronizationTargets.Select(st => st.GoogleAccountName).Distinct().ToArray();
            }

            await taskListLogger.LogAllTaskListsAsync(googleAccountNames);

            return new OkObjectResult($"Available Task Ids have been logged.");
        }
    }
}
