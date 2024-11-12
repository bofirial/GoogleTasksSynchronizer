using GoogleTasksSynchronizer.Configuration;
using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.DataAbstraction.Models
{
    public class TaskAccountGroup
    {
        public SynchronizationTarget SynchronizationTarget { get; set; }

        public List<Google::Task> Tasks { get; set; }
    }
}
