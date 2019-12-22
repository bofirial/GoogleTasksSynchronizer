using System.Threading.Tasks;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface IDeletedTasksProcessor
    {
        Task ProcessDeletedTasksAsync(MasterTaskGroup masterTaskGroup);
    }
}
