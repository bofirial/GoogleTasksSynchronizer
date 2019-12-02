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

        public async Task<List<MasterTask>> SelectAllAsync()
        {
            return (await _applicationStateManager.SelectAsync()).Tasks;
        }

        public async Task UpdateAsync(List<MasterTask> tasks)
        {
            var applicationState = await _applicationStateManager.SelectAsync();

            applicationState.Tasks = tasks;

            await _applicationStateManager.UpdateAsync(applicationState);
        }
    }
}
