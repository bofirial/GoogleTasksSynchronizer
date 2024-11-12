namespace GoogleTasksSynchronizer.Configuration
{
    public interface ISynchronizationTargetsProvider
    {
        Task<List<SynchronizationTarget>> GetAsync();
    }
}
