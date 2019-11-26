using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskBusinessManager : ITaskBusinessManager
    {
        public bool TasksMustBeCleared(params Task[] tasks)
        {
            var firstTask = tasks.FirstOrDefault();

            return !tasks.All(t => t.Hidden == firstTask.Hidden);
        }

        public bool TasksAreLogicallyEqual(params Task[] tasks)
        {
            var firstTask = tasks.FirstOrDefault();

            return tasks.All(t => t?.Title == firstTask?.Title) &&
                   tasks.All(t => t?.Due == firstTask?.Due) &&
                   tasks.All(t => t?.Notes == firstTask?.Notes) &&
                   tasks.All(t => t?.Status == firstTask?.Status) &&

                   tasks.All(t => t?.Deleted == firstTask?.Deleted) &&
                   tasks.All(t => t?.Completed == firstTask?.Completed);
        }

        public Task GetStoredTaskById(string taskId, TasksSynchronizerState tasksSynchronizerState)
        {
            foreach (var currentTask in tasksSynchronizerState.CurrentTasks)
            {
                if (currentTask.TaskIds.Any(t => t.TaskId == taskId))
                {
                    return currentTask.Task;
                }
            }

            return null;
        }

        public List<Task> RequestAllGoogleTasks(TasksResource.ListRequest listRequest)
        {
            var tasks = new List<Task>();

            Tasks taskResult = null;

            do
            {
                listRequest.PageToken = taskResult?.NextPageToken;

                taskResult = listRequest.Execute();

                if (null != taskResult?.Items)
                {
                    tasks.AddRange(taskResult.Items);
                }

            } while (taskResult?.NextPageToken != null);

            return tasks;
        }
    }
}
