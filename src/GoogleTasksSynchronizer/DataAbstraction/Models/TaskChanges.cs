using Google.Apis.Tasks.v1.Data;
using System.Collections.Generic;

namespace GoogleTasksSynchronizer.DataAbstraction.Models
{
    public class TaskChanges
    {
        public List<Task> TasksToCreate { get; set; }
        public List<Task> TasksToModify { get; set; }
        public List<Task> TasksToClear { get; set; }
    }
}