using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public class TaskBusinessManager : ITaskBusinessManager
    {
        private readonly ITaskManager _taskManager;

        public TaskBusinessManager(ITaskManager taskManager)
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

        public bool TasksAreEqual(MasterTask masterTask, Google::Task task)
        {
            masterTask = masterTask ?? throw new ArgumentNullException(nameof(masterTask));
            task = task ?? throw new ArgumentNullException(nameof(task));

            return masterTask.Title == task.Title &&
                    masterTask.Due == (task.Due != null ? (DateTime?)DateTime.Parse(task.Due, CultureInfo.InvariantCulture) : null) &&
                    masterTask.Notes == task.Notes &&
                    masterTask.Status == task.Status &&
                    masterTask.Deleted == task.Deleted &&
                    masterTask.Completed == (task.Completed != null ? (DateTime?)DateTime.Parse(task.Completed, CultureInfo.InvariantCulture) : null);
        }

        public bool ShouldSynchronizeTask(Google::Task task)
        {
            task = task ?? throw new ArgumentNullException(nameof(task));

            return !string.IsNullOrWhiteSpace(task.Title);
        }

        public async Task MoveAsync(Google::Task task, SynchronizationTarget synchronizationTarget, string previousTaskId)
        {
            await _taskManager.MoveAsync(task, synchronizationTarget, previousTaskId);
        }
    }
}
