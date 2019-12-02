using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface ITaskListLogger
    {
        Task LogAllTaskListsAsync(params string[] googleAccountNames);
    }
}
