using System.Collections.Generic;

namespace GoogleTasksSynchronizer.DataAbstraction.Models
{
    public class ApplicationState
    {
        public GoogleUserCredentials GoogleUserCredentials { get; set; } = new GoogleUserCredentials();

        public List<MasterTask> Tasks { get; set; } = new List<MasterTask>();
    }
}
