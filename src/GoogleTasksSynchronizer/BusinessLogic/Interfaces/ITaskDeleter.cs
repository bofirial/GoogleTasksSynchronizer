using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskDeleter
    {
        Task DeleteTaskAsync(MasterTask masterTask, List<TaskAccountGroup> taskAccountGroups);
    }
}
