﻿using GoogleTasksSynchronizer.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class TaskManager(
        ITaskServiceFactory taskServiceFactory,
        TelemetryConfiguration configuration) : ITaskManager
    {
        private readonly TelemetryClient _telemetryClient = new(configuration);

        public async Task<List<Google::Task>> SelectAllAsync(SynchronizationTarget synchronizationTarget)
        {
            synchronizationTarget = synchronizationTarget ?? throw new ArgumentNullException(nameof(synchronizationTarget));

            var taskService = await taskServiceFactory.CreateTaskServiceAsync(synchronizationTarget);

            var listRequest = taskService.Tasks.List(synchronizationTarget.TaskListId);

            listRequest.MaxResults = 100;

            listRequest.ShowCompleted = true;
            listRequest.ShowHidden = true;

            var tasks = new List<Google::Task>();

            Google::Tasks taskResult = null;

            do
            {
                listRequest.PageToken = taskResult?.NextPageToken;

                _telemetryClient.TrackEvent("GoogleAPICall", new Dictionary<string, string>() {
                    { "ApiMethod", "GetTasks" },
                    { "GoogleAccountName", synchronizationTarget.GoogleAccountName },
                    { "SynchronizationId", synchronizationTarget.SynchronizationId }
                });

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
            task = task ?? throw new ArgumentNullException(nameof(task));
            synchronizationTarget = synchronizationTarget ?? throw new ArgumentNullException(nameof(synchronizationTarget));

            var taskService = await taskServiceFactory.CreateTaskServiceAsync(synchronizationTarget);

            var insertRequest = taskService.Tasks.Insert(task, synchronizationTarget.TaskListId);

            _telemetryClient.TrackEvent("GoogleAPICall", new Dictionary<string, string>() {
                { "ApiMethod", "CreatedTask" },
                { "GoogleAccountName", synchronizationTarget.GoogleAccountName },
                { "SynchronizationId", synchronizationTarget.SynchronizationId },
                { "Task.Title", task.Title }
            });

            return await insertRequest.ExecuteAsync();
        }

        public async Task UpdateAsync(Google::Task task, SynchronizationTarget synchronizationTarget)
        {
            task = task ?? throw new ArgumentNullException(nameof(task));
            synchronizationTarget = synchronizationTarget ?? throw new ArgumentNullException(nameof(synchronizationTarget));

            var taskService = await taskServiceFactory.CreateTaskServiceAsync(synchronizationTarget);

            var updateRequest = taskService.Tasks.Update(task, synchronizationTarget.TaskListId, task.Id);

            _telemetryClient.TrackEvent("GoogleAPICall", new Dictionary<string, string>() {
                { "ApiMethod", "ModifiedTask" },
                { "GoogleAccountName", synchronizationTarget.GoogleAccountName },
                { "SynchronizationId", synchronizationTarget.SynchronizationId },
                { "Task.Title", task.Title }
            });

            await updateRequest.ExecuteAsync();
        }

        public async Task MoveAsync(Google::Task task, SynchronizationTarget synchronizationTarget, string previousTaskId)
        {
            task = task ?? throw new ArgumentNullException(nameof(task));
            synchronizationTarget = synchronizationTarget ?? throw new ArgumentNullException(nameof(synchronizationTarget));

            var taskService = await taskServiceFactory.CreateTaskServiceAsync(synchronizationTarget);

            var moveRequest = taskService.Tasks.Move(synchronizationTarget.TaskListId, task.Id);

            moveRequest.Previous = previousTaskId;

            _telemetryClient.TrackEvent("GoogleAPICall", new Dictionary<string, string>() {
                { "ApiMethod", "MoveTask" },
                { "GoogleAccountName", synchronizationTarget.GoogleAccountName },
                { "SynchronizationId", synchronizationTarget.SynchronizationId },
                { "Task.Title", task.Title }
            });

            await moveRequest.ExecuteAsync();
        }
    }
}
