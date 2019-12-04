using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class MasterTaskManager : IMasterTaskManager
    {
        private readonly IApplicationStateManager _applicationStateManager;
        private readonly TelemetryClient _telemetryClient;

        public MasterTaskManager(IApplicationStateManager applicationStateManager,
            TelemetryConfiguration configuration)
        {
            _applicationStateManager = applicationStateManager;
            _telemetryClient = new TelemetryClient(configuration);
        }

        public async Task<List<MasterTask>> SelectAllAsync(string synchronizationId)
        {
            var tasksDictionary = (await _applicationStateManager.SelectAsync()).Tasks;

            if (!tasksDictionary.ContainsKey(synchronizationId))
            {
                return new List<MasterTask>();
            }

            return tasksDictionary[synchronizationId];
        }

        public async Task UpdateAsync(string synchronizationId, List<MasterTask> tasks)
        {
            tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));

            _telemetryClient.TrackEvent("UpdateMasterTasks", new Dictionary<string, string>() {
                    { "SynchronizationId", synchronizationId },
                    { "TotalMasterTasks", tasks.Count.ToString(CultureInfo.InvariantCulture) }
                });

            var applicationState = await _applicationStateManager.SelectAsync();

            applicationState.Tasks[synchronizationId] = tasks;

            await _applicationStateManager.UpdateAsync(applicationState);
        }
    }
}
