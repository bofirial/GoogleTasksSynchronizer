using GoogleTasksSynchronizer.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class TaskListLogger : ITaskListLogger
    {
        private readonly ILogger<TaskListLogger> _logger;
        private readonly ITaskServiceFactory _taskServiceFactory;
        private readonly TelemetryClient _telemetryClient;

        public TaskListLogger(ILogger<TaskListLogger> logger, ITaskServiceFactory taskServiceFactory, TelemetryConfiguration configuration)
        {
            _logger = logger;
            _taskServiceFactory = taskServiceFactory;
            _telemetryClient = new TelemetryClient(configuration);
        }

        public async Task LogAllTaskListsAsync(string[] googleAccountNames)
        {
            foreach (var googleAccountName in googleAccountNames)
            {
                var taskService = await _taskServiceFactory.CreateTaskServiceAsync(new SynchronizationTarget() { GoogleAccountName = googleAccountName });

                var taskListRequest = taskService.Tasklists.List();

                _telemetryClient.TrackEvent("GoogleAPICall");
                var response = await taskListRequest.ExecuteAsync();

                foreach (var item in response.Items)
                {
                    _logger.LogInformation($"{googleAccountName} has a task list named \"{item.Title}\" with an Id of \"{item.Id}\"");
                }
            }
        }
    }
}
