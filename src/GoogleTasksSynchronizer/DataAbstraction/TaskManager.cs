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
        private readonly ISingleExecutionEnforcementExecutor _singleExecutionEnforcementExecutor;
        private readonly TelemetryClient _telemetryClient;

        public TaskManager(
            ITaskServiceFactory taskServiceFactory, 
            TelemetryConfiguration configuration, 
            ISingleExecutionEnforcementExecutor singleExecutionEnforcementExecutor)
        {
            _taskServiceFactory = taskServiceFactory;
            _singleExecutionEnforcementExecutor = singleExecutionEnforcementExecutor;
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

            Google::Tasks taskResult = null;

            do
            {
                listRequest.PageToken = taskResult?.NextPageToken;

                _telemetryClient.TrackEvent("GoogleAPICall");
                _telemetryClient.TrackEvent("GetTasks");
                taskResult = await listRequest.ExecuteAsync();

                if (null != taskResult?.Items)
                {
                    tasks.AddRange(taskResult.Items);
                }

            } while (taskResult?.NextPageToken != null);

            return tasks;
        }

        public async Task<Google::Task> InsertAsync(Google::Task task, SynchronizationTarget synchronizationTarget)
        {
            var taskService = await _taskServiceFactory.CreateTaskServiceAsync(synchronizationTarget);

            var insertRequest = taskService.Tasks.Insert(task, synchronizationTarget.TaskListId);

            _telemetryClient.TrackEvent("GoogleAPICall");
            _telemetryClient.TrackEvent("CreatedTask");
            return await insertRequest.ExecuteAsync();
        }

        public async Task UpdateAsync(Google::Task task, SynchronizationTarget synchronizationTarget)
        {
            var taskService = await _taskServiceFactory.CreateTaskServiceAsync(synchronizationTarget);

            var updateRequest = taskService.Tasks.Update(task, synchronizationTarget.TaskListId, task.Id);

            _telemetryClient.TrackEvent("GoogleAPICall");
            _telemetryClient.TrackEvent("ModifiedTask");
            await updateRequest.ExecuteAsync();
        }

        public async Task ClearAsync(Google::Task task, SynchronizationTarget synchronizationTarget)
        {
            await _singleExecutionEnforcementExecutor.ExecuteOnceAsync(async taskListId =>
            {
                var taskService = await _taskServiceFactory.CreateTaskServiceAsync(synchronizationTarget);

                _telemetryClient.TrackEvent("GoogleAPICall");
                _telemetryClient.TrackEvent("ClearTasks");
                await taskService.Tasks.Clear(taskListId).ExecuteAsync();
            }, synchronizationTarget.TaskListId);
        }
    }
}
