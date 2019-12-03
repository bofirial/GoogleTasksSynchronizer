﻿using GoogleTasksSynchronizer.Configuration;
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

            Google::Tasks taskResult = null;

            do
            {
                listRequest.PageToken = taskResult?.NextPageToken;

                _telemetryClient.TrackEvent("GoogleAPICall");
                taskResult = listRequest.Execute();

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
            return insertRequest.Execute();
        }

        public async Task UpdateAsync(Google::Task task, SynchronizationTarget synchronizationTarget)
        {
            var taskService = await _taskServiceFactory.CreateTaskServiceAsync(synchronizationTarget);

            var updateRequest = taskService.Tasks.Update(task, synchronizationTarget.TaskListId, task.Id);

            _telemetryClient.TrackEvent("GoogleAPICall");
            _telemetryClient.TrackEvent("ModifiedTask");
            updateRequest.Execute();
        }

        public async Task ClearAsync(Google::Task task, SynchronizationTarget synchronizationTarget)
        { 
            //TODO: Protect from multiple clearings in a single run because each clear clears all completed tasks
            var taskService = await _taskServiceFactory.CreateTaskServiceAsync(synchronizationTarget);

            _telemetryClient.TrackEvent("GoogleAPICall");
            _telemetryClient.TrackEvent("ClearTasks");
            taskService.Tasks.Clear(synchronizationTarget.TaskListId).Execute();
        }
    }
}
