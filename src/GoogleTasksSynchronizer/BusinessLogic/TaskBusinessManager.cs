using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskBusinessManager : ITaskBusinessManager
    {
        private readonly TasksSynchronizerState _tasksSynchronizerState;

        public TaskBusinessManager(TasksSynchronizerState tasksSynchronizerState)
        {
            _tasksSynchronizerState = tasksSynchronizerState;
        }

        public bool TasksMustBeCleared(params Task[] tasks)
        {
            Task firstTask = tasks.FirstOrDefault();

            return !tasks.All(t => t.Hidden == firstTask.Hidden);
        }

        public bool TasksAreLogicallyEqual(params Task[] tasks)
        {
            Task firstTask = tasks.FirstOrDefault();

            return tasks.All(t => t.Title == firstTask?.Title) &&
                   tasks.All(t => t.Due == firstTask?.Due) &&
                   tasks.All(t => t.Notes == firstTask?.Notes) &&
                   tasks.All(t => t.Status == firstTask?.Status) &&

                   tasks.All(t => t.Deleted == firstTask?.Deleted) &&
                   tasks.All(t => t.Completed == firstTask?.Completed);
        }

        public Task GetStoredTaskById(string taskId)
        {
            foreach (CurrentTask currentTask in _tasksSynchronizerState.CurrentTasks)
            {
                if (currentTask.TaskIds.Any(t => t.TaskId == taskId))
                {
                    return currentTask.Task;
                }
            }

            return null;
        }
    }
}
