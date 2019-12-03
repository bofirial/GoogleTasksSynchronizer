using GoogleTasksSynchronizer.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface ITaskManager
    {
        Task<List<Google::Task>> SelectAllAsync(SynchronizationTarget synchronizationTarget);

        Task<Google::Task> InsertAsync(Google::Task task, SynchronizationTarget synchronizationTarget);

        Task UpdateAsync(Google::Task task, SynchronizationTarget synchronizationTarget);
    }
}
