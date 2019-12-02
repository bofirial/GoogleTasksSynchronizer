using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.Configuration
{
    public interface ISynchronizationTargetManager
    {
        Task<List<SynchronizationTarget>> SelectAllAsync();
    }
}
