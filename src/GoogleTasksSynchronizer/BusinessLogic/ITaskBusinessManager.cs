using System.Collections;
using System.Collections.Generic;
using Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskBusinessManager
    {
        bool TasksMustBeCleared(params Task[] tasks);

        bool TasksAreLogicallyEqual(params Task[] tasks);

        Task GetStoredTaskById(string taskId);
    }
}