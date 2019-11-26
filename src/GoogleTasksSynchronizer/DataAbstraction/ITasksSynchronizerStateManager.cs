using GoogleTasksSynchronizer.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface ITasksSynchronizerStateManager
    {
        Task<TasksSynchronizerState> SelectTasksSynchronizerStateAsync(CloudBlockBlob cloudBlockBlob);

        Task UpdateTasksSynchronizerStateAsync(TasksSynchronizerState tasksSynchronizerState, CloudBlockBlob tasksSynchronizerStateBlob);
    }
}
