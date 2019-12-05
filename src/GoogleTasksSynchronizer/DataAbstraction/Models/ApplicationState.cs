using System.Collections.Generic;

namespace GoogleTasksSynchronizer.DataAbstraction.Models
{
    public class ApplicationState
    {
        public GoogleUserCredentialsDictionary GoogleUserCredentials { get; set; } = new GoogleUserCredentialsDictionary();

        public Dictionary<string, List<MasterTask>> Tasks { get; set; } = new Dictionary<string, List<MasterTask>>();
    }
}
