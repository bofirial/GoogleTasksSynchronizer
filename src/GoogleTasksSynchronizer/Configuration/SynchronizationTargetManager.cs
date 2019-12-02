using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.Configuration
{
    public class SynchronizationTargetManager : ISynchronizationTargetManager
    {
        private readonly IOptions<SynchronizationTargetsOptions> _synchronizationTargetsOptions;

        public SynchronizationTargetManager(
            IOptions<SynchronizationTargetsOptions> synchronizationTargetsOptions)
        {
            _synchronizationTargetsOptions = synchronizationTargetsOptions;
        }

        public Task<List<SynchronizationTarget>> SelectAllAsync()
        {
            return Task.FromResult(JsonConvert.DeserializeObject<List<SynchronizationTarget>>(_synchronizationTargetsOptions.Value.SynchronizationTargets));
        }
    }
}
