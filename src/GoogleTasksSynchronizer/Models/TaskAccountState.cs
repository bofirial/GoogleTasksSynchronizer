using Google.Apis.Auth.OAuth2;
using Google = Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.Configuration;
using System.Collections.Generic;

namespace GoogleTasksSynchronizer.Models
{
    public class TaskAccountState
    {
        public SynchronizationTarget SynchronizationTarget { get; set; }

        internal UserCredential UserCredential { get; set; }

        public List<Google::Task> GoogleTasks { get; set; }
    }
}
