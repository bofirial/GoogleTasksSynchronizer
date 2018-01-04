using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleTasksSynchronizer.Models
{
    public class TasksSynchronizerState
    {
        public Dictionary<string, string> GoogleUserCredentials { get; set; } = new Dictionary<string, string>();

        public DateTime LastQueryTime { get; set; }
     }
}
