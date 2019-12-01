using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleTasksSynchronizer.DataAbstraction.Models
{
    public class MasterTask
    {
        public string Title  { get; set; }
        public DateTime? Due { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public bool Deleted { get; set; }
        public DateTime? Completed { get; set; }
    }
}