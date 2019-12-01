using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public interface IMasterTaskBusinessManager
    {
        Task<List<MasterTask>> SelectAllAsync();

        Task UpdateAsync(List<MasterTask> tasks);
    }
}
