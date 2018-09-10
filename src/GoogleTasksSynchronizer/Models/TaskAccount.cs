using System;
using System.Collections.Generic;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.Models
{
    public class TaskAccount
    {
        public string AccountName { get; set; }

        public string TaskListId { get; set; }

        internal UserCredential UserCredential { get; set; }

        public List<Task> GoogleTasks { get; set; }
    }
}
