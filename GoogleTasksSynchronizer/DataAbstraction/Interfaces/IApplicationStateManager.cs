using Azure.Storage.Blobs;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface IApplicationStateManager
    {
        Task InitializeFromBindingAsync(BlobClient applicationStateBlob);

        Task<ApplicationState> SelectAsync();

        Task UpdateAsync(ApplicationState applicationState);
    }
}
