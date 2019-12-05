using System.Threading.Tasks;
using Google.Apis.Tasks.v1;
using GoogleTasksSynchronizer.Configuration;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface ITaskServiceFactory
    {
        Task<TasksService> CreateTaskServiceAsync(SynchronizationTarget synchronizationTarget);
    }
}
