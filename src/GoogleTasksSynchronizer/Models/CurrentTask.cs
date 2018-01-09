using System;
using System.Collections.Generic;
using System.Text;
using Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.Models
{
    public class CurrentTask
    {
        public Task Task { get; set; }

        public List<TaskIdentifier> TaskIds { get; set; }
    }
}
