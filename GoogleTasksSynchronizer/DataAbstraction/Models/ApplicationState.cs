namespace GoogleTasksSynchronizer.DataAbstraction.Models
{
    public class ApplicationState
    {
        public GoogleUserCredentialsDictionary GoogleUserCredentials { get; set; } = [];

        public Dictionary<string, List<MasterTask>> Tasks { get; set; } = [];
    }
}
