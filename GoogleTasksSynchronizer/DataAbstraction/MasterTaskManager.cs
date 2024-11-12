using System.Globalization;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class MasterTaskManager(IApplicationStateManager applicationStateManager,
        TelemetryConfiguration configuration) : IMasterTaskManager
    {
        private readonly TelemetryClient _telemetryClient = new(configuration);

        public async Task<List<MasterTask>> SelectAllAsync(string synchronizationId)
        {
            var tasksDictionary = (await applicationStateManager.SelectAsync()).Tasks;

            return tasksDictionary.TryGetValue(synchronizationId, out var value) ? value : ([]);
        }

        public async Task UpdateAsync(string synchronizationId, List<MasterTask> tasks)
        {
            tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));

            _telemetryClient.TrackEvent("UpdateMasterTasks", new Dictionary<string, string>() {
                    { "SynchronizationId", synchronizationId },
                    { "TotalMasterTasks", tasks.Count.ToString(CultureInfo.InvariantCulture) }
                });

            var applicationState = await applicationStateManager.SelectAsync();

            applicationState.Tasks[synchronizationId] = tasks;

            await applicationStateManager.UpdateAsync(applicationState);
        }
    }
}
