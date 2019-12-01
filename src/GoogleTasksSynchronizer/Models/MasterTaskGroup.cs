using GoogleTasksSynchronizer.DataAbstraction.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleTasksSynchronizer.Models
{
    public class MasterTaskGroup
    {
        public string SynchronizationId { get; set; }

        public List<MasterTask> MasterTasks { get; set; }

        public List<TaskAccountGroup> TaskAccountGroups { get; set; }
    }
}
