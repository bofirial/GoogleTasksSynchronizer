using System.Collections.Generic;

namespace GoogleTasksSynchronizer.DataAbstraction.Models
{
    public class ApplicationState
    {
        public GoogleUserCredentials GoogleUserCredentials { get; set; } = new GoogleUserCredentials();

        public Dictionary<string, List<MasterTask>> Tasks { get; set; } = new Dictionary<string, List<MasterTask>>();
    }
}
