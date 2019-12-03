using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class TaskManager : ITaskManager
    {
        private readonly ITaskServiceFactory _taskServiceFactory;
        private readonly TelemetryClient _telemetryClient;

        public TaskManager(ITaskServiceFactory taskServiceFactory, TelemetryConfiguration configuration)
        {
            _taskServiceFactory = taskServiceFactory;
            _telemetryClient = new TelemetryClient(configuration);
        }

        public async Task<List<Google::Task>> SelectAllAsync(SynchronizationTarget synchronizationTarget)
        {
            var taskService = await _taskServiceFactory.CreateTaskServiceAsync(synchronizationTarget);

            var listRequest = taskService.Tasks.List(synchronizationTarget.TaskListId);

            listRequest.ShowCompleted = true;
            listRequest.ShowDeleted = true;
            listRequest.ShowHidden = true;

            listRequest.UpdatedMin = DateTime.Today.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

            var tasks = new List<Google::Task>();

            Tasks taskResult = null;

            do
            {
                listRequest.PageToken = taskResult?.NextPageToken;

                taskResult = listRequest.Execute();
                _telemetryClient.TrackEvent("GoogleAPICall");

                if (null != taskResult?.Items)
                {
                    tasks.AddRange(taskResult.Items);
                }

            } while (taskResult?.NextPageToken != null);

            return tasks;
        }
    }
}
