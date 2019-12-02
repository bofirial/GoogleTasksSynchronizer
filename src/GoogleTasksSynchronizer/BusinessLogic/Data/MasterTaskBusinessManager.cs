using GoogleTasksSynchronizer.DataAbstraction;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public class MasterTaskBusinessManager : IMasterTaskBusinessManager
    {
        private readonly IMasterTaskManager _masterTaskManager;

        public MasterTaskBusinessManager(IMasterTaskManager masterTaskManager)
        {
            _masterTaskManager = masterTaskManager;
        }

        public Task<List<MasterTask>> SelectAllAsync()
        {
            return _masterTaskManager.SelectAllAsync();
        }

        public Task UpdateAsync(List<MasterTask> tasks)
        {
            return _masterTaskManager.UpdateAsync(tasks);
        }
    }
}
