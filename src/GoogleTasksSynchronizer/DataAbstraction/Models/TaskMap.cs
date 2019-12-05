using GoogleTasksSynchronizer.Configuration;

namespace GoogleTasksSynchronizer.DataAbstraction.Models
{
    public class TaskMap
    {
        public string TaskId { get; set; }

        public SynchronizationTarget SynchronizationTarget { get; set; }
    }
}
