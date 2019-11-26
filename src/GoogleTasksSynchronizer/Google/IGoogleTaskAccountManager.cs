using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.Models;

namespace GoogleTasksSynchronizer.Google
{
    public interface IGoogleTaskAccountManager
    {
        Task<List<TaskAccountState>> GetTaskAccountsAsync(TasksSynchronizerState tasksSynchronizerState);
    }
}
