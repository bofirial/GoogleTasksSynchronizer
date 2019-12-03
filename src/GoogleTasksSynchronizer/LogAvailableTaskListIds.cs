using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using GoogleTasksSynchronizer.DataAbstraction;
using GoogleTasksSynchronizer.Configuration;
using System.Linq;

namespace GoogleTasksSynchronizer
{
    public class LogAvailableTaskListIds
    {
        private readonly ISynchronizationTargetsProvider _synchronizationTargetManager;
        private readonly ITaskListLogger _taskListLogger;

        public LogAvailableTaskListIds(ISynchronizationTargetsProvider synchronizationTargetManager, ITaskListLogger taskListLogger)
        {
            _synchronizationTargetManager = synchronizationTargetManager;
            _taskListLogger = taskListLogger;
        }

        [FunctionName("LogAvailableTaskListIds")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            string[] googleAccountNames = req.Query["googleAccountName"];

            if (null == googleAccountNames || googleAccountNames.Length == 0)
            {
                var synchronizationTargets = await _synchronizationTargetManager.GetAsync();

                googleAccountNames = synchronizationTargets.Select(st => st.GoogleAccountName).Distinct().ToArray();
            }

            await _taskListLogger.LogAllTaskListsAsync(googleAccountNames);

            return new OkObjectResult($"Available Task Ids have been logged.");
        }
    }
}
