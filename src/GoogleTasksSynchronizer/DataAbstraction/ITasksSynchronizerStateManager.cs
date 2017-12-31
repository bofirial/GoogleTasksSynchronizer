using GoogleTasksSynchronizer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface ITasksSynchronizerStateManager
    {
        Task<TasksSynchronizerState> SelectTasksSynchronizerStateAsync();

        Task UpdateTasksSynchronizerStateAsync(TasksSynchronizerState tasksSynchronizerState);
    }
}
