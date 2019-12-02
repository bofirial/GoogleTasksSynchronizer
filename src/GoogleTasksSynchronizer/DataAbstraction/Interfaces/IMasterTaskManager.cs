using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface IMasterTaskManager
    {
        Task<List<MasterTask>> SelectAllAsync();

        Task UpdateAsync(List<MasterTask> tasks);
    }
}
