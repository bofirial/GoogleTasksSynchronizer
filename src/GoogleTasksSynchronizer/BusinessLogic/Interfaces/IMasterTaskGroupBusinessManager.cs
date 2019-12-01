using GoogleTasksSynchronizer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface IMasterTaskGroupBusinessManager
    {
        Task<List<MasterTaskGroup>> SelectAsync();
    }
}
