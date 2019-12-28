using System.Threading.Tasks;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface IDeletedTasksProcessor
    {
        Task<bool> ProcessDeletedTasksAsync(MasterTaskGroup masterTaskGroup);
    }
}
