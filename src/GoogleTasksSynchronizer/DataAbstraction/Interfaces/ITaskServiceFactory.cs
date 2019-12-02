using Google.Apis.Tasks.v1;
using GoogleTasksSynchronizer.Configuration;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface ITaskServiceFactory
    {
        Task<TasksService> CreateTaskServiceAsync(SynchronizationTarget synchronizationTarget);
    }
}
