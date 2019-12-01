using GoogleTasksSynchronizer.DataAbstraction.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.BusinessLogic.Data
{
    public class MasterTaskBusinessManager : IMasterTaskBusinessManager
    {
        public Task<List<MasterTask>> SelectAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(List<MasterTask> tasks)
        {
            throw new NotImplementedException();
        }
    }
}
