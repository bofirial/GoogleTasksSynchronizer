using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public interface ITaskBusinessManager
    {
        Task<List<Google::Task>> SelectAllAsync(SynchronizationTarget synchronizationTarget);

        Task<Google::Task> InsertAsync(Google::Task task, SynchronizationTarget synchronizationTarget);

        Task UpdateAsync(Google::Task task, SynchronizationTarget synchronizationTarget);

        Task MoveAsync(Google::Task task, SynchronizationTarget synchronizationTarget, string previousTaskId);

        bool TasksAreEqual(MasterTask masterTask, Google::Task task);

        bool ShouldSynchronizeTask(Google::Task task);
    }
}
