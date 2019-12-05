using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskCreator
    {
        Task<MasterTask> CreateNewTaskAsync(Google::Task task, List<TaskAccountGroup> taskAccountGroups);
    }
}
