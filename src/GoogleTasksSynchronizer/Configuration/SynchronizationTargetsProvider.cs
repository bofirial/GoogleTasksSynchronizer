using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer.Configuration
{
    public class SynchronizationTargetsProvider : ISynchronizationTargetsProvider
    {
        private readonly IOptions<SynchronizationTargetsOptions> _synchronizationTargetsOptions;

        public SynchronizationTargetsProvider(
            IOptions<SynchronizationTargetsOptions> synchronizationTargetsOptions)
        {
            _synchronizationTargetsOptions = synchronizationTargetsOptions;
        }

        public Task<List<SynchronizationTarget>> GetAsync()
        {
            var synchronizationTargets = JsonConvert.DeserializeObject<List<SynchronizationTarget>>(_synchronizationTargetsOptions.Value.SynchronizationTargets);

            ValidateSynchronizationTargets(synchronizationTargets);

            return Task.FromResult(synchronizationTargets);
        }

        private void ValidateSynchronizationTargets(List<SynchronizationTarget> synchronizationTargets)
        {
            if (synchronizationTargets.GroupBy(s => s.TaskListId).Any(t => t.Count() > 1))
            {
                throw new Exception("Invalid Configuration: TaskListId can only be used once in SyncronizationTargets");
            }
        }
    }
}
