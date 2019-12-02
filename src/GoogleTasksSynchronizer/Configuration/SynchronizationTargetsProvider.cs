using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            return Task.FromResult(JsonConvert.DeserializeObject<List<SynchronizationTarget>>(_synchronizationTargetsOptions.Value.SynchronizationTargets));
        }
    }
}
