using System;
using System.Collections.Generic;
using System.Text;
using Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.Models
{
    public class TasksSynchronizerState
    {
        public Dictionary<string, string> GoogleUserCredentials { get; set; } = new Dictionary<string, string>();

        public DateTime LastQueryTime { get; set; }

        public List<CurrentTask> CurrentTasks { get; set; } = new List<CurrentTask>();
    }
}
