using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using GoogleTasksSynchronizer.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskServiceFactory : ITaskServiceFactory
    {
        public TasksService CreateTaskService(TaskAccount taskAccount)
        {
            return new TasksService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = taskAccount.UserCredential,
                ApplicationName = "JSchafer Google Tasks Synchronizer",
            });
        }
    }
}
