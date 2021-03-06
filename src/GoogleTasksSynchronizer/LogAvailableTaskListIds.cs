using System;
using System.Linq;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

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
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));

            string[] googleAccountNames = request.Query["googleAccountName"];

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
