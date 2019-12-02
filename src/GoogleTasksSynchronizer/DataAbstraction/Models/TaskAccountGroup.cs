using Google = Google.Apis.Tasks.v1.Data;
using System.Collections.Generic;

namespace GoogleTasksSynchronizer.DataAbstraction.Models
{

    public class TaskAccountGroup
    {
        public string GoogleAccountName { get; set; }

        public string TaskListId { get; set; }

        public List<Google::Task> Tasks { get; set; }
    }
}
