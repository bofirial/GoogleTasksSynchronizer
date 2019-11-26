using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskChangeCalculator
    {
        void CalculateTaskChanges(TaskAccount taskAccount, TasksSynchronizerState tasksSynchronizerState, List<Task> createdTasks, List<Task> modifiedTasks, List<Task> clearedTasks);
    }
}
