using GoogleTasksSynchronizer.Models;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskChangesProcessor
    {
        Task ProcessTaskChangesAsync();
    }
}
