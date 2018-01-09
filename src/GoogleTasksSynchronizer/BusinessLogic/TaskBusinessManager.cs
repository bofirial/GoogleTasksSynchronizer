using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskBusinessManager : ITaskBusinessManager
    {
        public bool TasksAreLogicallyEqual(params Task[] tasks)
        {
            Task firstTask = tasks.FirstOrDefault();

            return tasks.All(t => t.Title == firstTask?.Title) &&
                   tasks.All(t => t.Due == firstTask?.Due) &&
                   tasks.All(t => t.Notes == firstTask?.Notes) &&
                   tasks.All(t => t.Status == firstTask?.Status);
        }
    }
}
