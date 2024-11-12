namespace GoogleTasksSynchronizer.DataAbstraction.Models
{
    public class MasterTaskGroup
    {
        public string SynchronizationId { get; set; }

        public List<MasterTask> MasterTasks { get; set; }

        public List<TaskAccountGroup> TaskAccountGroups { get; set; }
    }
}
