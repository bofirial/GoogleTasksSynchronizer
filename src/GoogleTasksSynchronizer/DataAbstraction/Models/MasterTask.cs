using System;
using System.Collections.Generic;

namespace GoogleTasksSynchronizer.DataAbstraction.Models
{
    public class MasterTask
    {
        public string MasterTaskId { get; set; }
        public List<TaskMap> TaskMaps { get; set; }
        public DateTime UpdatedOn { get; set; }

        public string Title  { get; set; }
        public DateTime? Due { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public bool? Deleted { get; set; }
        public DateTime? Completed { get; set; }
    }
}