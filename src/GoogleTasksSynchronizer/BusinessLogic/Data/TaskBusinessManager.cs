﻿using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public class TaskBusinessManager : ITaskBusinessManager
    {
        private readonly ITaskManager _taskManager;

        public TaskBusinessManager(
            ITaskManager taskManager
            )
        {
            _taskManager = taskManager;
        }

        public async Task<List<Google::Task>> SelectAllAsync(SynchronizationTarget synchronizationTarget)
        {
            return await _taskManager.SelectAllAsync(synchronizationTarget);
        }

        public async Task<Google::Task> InsertAsync(Google::Task task, SynchronizationTarget synchronizationTarget)
        {
            return await _taskManager.InsertAsync(task, synchronizationTarget);
        }

        public async Task UpdateAsync(Google::Task task, SynchronizationTarget synchronizationTarget)
        {
            await _taskManager.UpdateAsync(task, synchronizationTarget);
        }

        public async Task ClearAsync(Google::Task task, SynchronizationTarget synchronizationTarget)
        {
            await _taskManager.ClearAsync(task, synchronizationTarget);
        }
    }
}
