using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface IMasterTaskGroupBusinessManager
    {
        Task<List<MasterTaskGroup>> SelectAsync();
    }
}
