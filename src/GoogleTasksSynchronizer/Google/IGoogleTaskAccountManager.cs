using System;
using System.Collections.Generic;
using System.Text;
using GoogleTasksSynchronizer.Models;

namespace GoogleTasksSynchronizer.Google
{
    public interface IGoogleTaskAccountManager
    {
        List<TaskAccount> GetTaskAccounts(TasksSynchronizerState tasksSynchronizerState);
    }
}
