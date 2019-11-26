using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.Models;
using System.Collections.Generic;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskChangeCalculator
    {
        void CalculateTaskChanges(TaskAccount taskAccount, 
            TasksSynchronizerState tasksSynchronizerState, 
            List<Task> createdTasks, 
            List<Task> modifiedTasks, 
            List<Task> clearedTasks);
    }
}
