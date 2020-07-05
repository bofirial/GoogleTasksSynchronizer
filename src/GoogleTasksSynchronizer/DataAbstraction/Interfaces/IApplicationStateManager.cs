using System.Threading.Tasks;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Azure.Storage.Blob;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface IApplicationStateManager
    {
        Task InitializeFromBindingAsync(CloudBlockBlob applicationStateBlob);

        Task<ApplicationState> SelectAsync();

        Task UpdateAsync(ApplicationState applicationState);
    }
}
