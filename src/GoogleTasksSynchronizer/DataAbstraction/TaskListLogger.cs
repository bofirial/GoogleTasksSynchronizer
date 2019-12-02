using GoogleTasksSynchronizer.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class TaskListLogger : ITaskListLogger
    {
        private readonly ILogger<TaskListLogger> _logger;
        private readonly ITaskServiceFactory _taskServiceFactory;

        public TaskListLogger(ILogger<TaskListLogger> logger, ITaskServiceFactory taskServiceFactory)
        {
            _logger = logger;
            _taskServiceFactory = taskServiceFactory;
        }

        public async Task LogAllTaskListsAsync(string[] googleAccountNames)
        {
            foreach (var googleAccountName in googleAccountNames)
            {
                var taskService = await _taskServiceFactory.CreateTaskServiceAsync(new SynchronizationTarget() { GoogleAccountName = googleAccountName });

                var taskListRequest = taskService.Tasklists.List();

                var response = await taskListRequest.ExecuteAsync();

                foreach (var item in response.Items)
                {
                    _logger.LogInformation($"{googleAccountName} has a task list named \"{item.Title}\" with an Id of \"{item.Id}\"");
                }
            }
        }
    }
}
