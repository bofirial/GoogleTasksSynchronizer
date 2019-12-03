using Google = Google.Apis.Tasks.v1.Data;
using System.Collections.Generic;
using GoogleTasksSynchronizer.Configuration;

namespace GoogleTasksSynchronizer.DataAbstraction.Models
{

    public class TaskAccountGroup
    {
        public SynchronizationTarget SynchronizationTarget { get; set; }

        public List<Google::Task> Tasks { get; set; }
    }
}
