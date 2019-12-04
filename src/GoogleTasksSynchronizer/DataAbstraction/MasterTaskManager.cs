using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public class MasterTaskManager : IMasterTaskManager
    {
        private readonly IApplicationStateManager _applicationStateManager;

        public MasterTaskManager(IApplicationStateManager applicationStateManager)
        {
            _applicationStateManager = applicationStateManager;
        }

        public async Task<List<MasterTask>> SelectAllAsync(string synchronizationId)
        {
            var tasksDictionary = (await _applicationStateManager.SelectAsync()).Tasks;

            //TODO: What if the dictionary doesn't exist yet?

            return tasksDictionary[synchronizationId];
        }

        public async Task UpdateAsync(string synchronizationId, List<MasterTask> tasks)
        {
            var applicationState = await _applicationStateManager.SelectAsync();

            applicationState.Tasks[synchronizationId] = tasks;

            await _applicationStateManager.UpdateAsync(applicationState);
        }
    }
}
