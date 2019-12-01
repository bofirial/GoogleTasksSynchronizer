using System.Collections;
using System.Collections.Generic;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface IOldTaskBusinessManager
    {
        bool TasksMustBeCleared(params Task[] tasks);

        bool TasksAreLogicallyEqual(params Task[] tasks);

        Task GetStoredTaskById(string taskId, TasksSynchronizerState tasksSynchronizerState);

        List<Task> RequestAllGoogleTasks(TasksResource.ListRequest listRequest);
    }
}