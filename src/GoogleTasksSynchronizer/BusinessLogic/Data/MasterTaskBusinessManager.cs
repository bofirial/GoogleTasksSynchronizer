using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.DataAbstraction;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public class MasterTaskBusinessManager : IMasterTaskBusinessManager
    {
        private readonly IMasterTaskManager _masterTaskManager;

        public MasterTaskBusinessManager(IMasterTaskManager masterTaskManager)
        {
            _masterTaskManager = masterTaskManager;
        }

        public Task<List<MasterTask>> SelectAllAsync(string synchronizationId)
        {
            return _masterTaskManager.SelectAllAsync(synchronizationId);
        }

        public Task UpdateAsync(string synchronizationId, List<MasterTask> tasks)
        {
            return _masterTaskManager.UpdateAsync(synchronizationId, tasks);
        }
    }
}
