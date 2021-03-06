﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

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
            googleAccountNames = googleAccountNames ?? throw new ArgumentNullException(nameof(googleAccountNames));

            foreach (var googleAccountName in googleAccountNames)
            {
                var taskService = await _taskServiceFactory.CreateTaskServiceAsync(new SynchronizationTarget() { GoogleAccountName = googleAccountName });

                var taskListRequest = taskService.Tasklists.List();

                _telemetryClient.TrackEvent("GoogleAPICall", new Dictionary<string, string>() {
                    { "ApiMethod", "GetTaskLists" },
                    { "GoogleAccount", googleAccountName }
                });

                var response = await taskListRequest.ExecuteAsync();

                foreach (var item in response.Items)
                {
                    _logger.LogInformation($"{googleAccountName} has a task list named \"{item.Title}\" with an Id of \"{item.Id}\"");
                }
            }
        }
    }
}
