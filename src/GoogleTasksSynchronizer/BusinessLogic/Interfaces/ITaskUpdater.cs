using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskUpdater
    {
        Task UpdateTaskAsync(Google::Task task, MasterTask masterTask, List<TaskAccountGroup> taskAccountGroups);
    }
}
