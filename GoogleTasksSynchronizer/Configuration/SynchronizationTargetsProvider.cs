using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer.Configuration
{
    public class SynchronizationTargetsProvider(
        IOptions<SynchronizationTargetsOptions> synchronizationTargetsOptions) : ISynchronizationTargetsProvider
    {
        public Task<List<SynchronizationTarget>> GetAsync()
        {
            var synchronizationTargets = JsonConvert.DeserializeObject<List<SynchronizationTarget>>(synchronizationTargetsOptions.Value.SynchronizationTargets);

            ValidateSynchronizationTargets(synchronizationTargets);

            return Task.FromResult(synchronizationTargets);
        }

        private static void ValidateSynchronizationTargets(List<SynchronizationTarget> synchronizationTargets)
        {
            if (synchronizationTargets.GroupBy(s => s.TaskListId).Any(t => t.Count() > 1))
            {
                throw new Exception("Invalid Configuration: TaskListId can only be used once in SyncronizationTargets");
            }
        }
    }
}
