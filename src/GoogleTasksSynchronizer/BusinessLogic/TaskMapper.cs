using System;
using System.Globalization;
using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Newtonsoft.Json;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskMapper : ITaskMapper
    {
        public void MapTask(Task toTask, MasterTask fromTask)
        {
            toTask = toTask ?? throw new ArgumentNullException(nameof(toTask));
            fromTask = fromTask ?? throw new ArgumentNullException(nameof(fromTask));

            toTask.Title = fromTask.Title;
            toTask.Due = JsonConvert.SerializeObject(fromTask.Due);
            toTask.Notes = fromTask.Notes;
            toTask.Status = fromTask.Status;
            toTask.Deleted = fromTask.Deleted;
            toTask.Completed = JsonConvert.SerializeObject(fromTask.Completed);
        }

        public void MapTask(MasterTask toTask, Task fromTask)
        {
            toTask = toTask ?? throw new ArgumentNullException(nameof(toTask));
            fromTask = fromTask ?? throw new ArgumentNullException(nameof(fromTask));

            toTask.Title = fromTask.Title;
            toTask.Due = DateTime.Parse(fromTask.Due, CultureInfo.InvariantCulture);
            toTask.Notes = fromTask.Notes;
            toTask.Status = fromTask.Status;
            toTask.Deleted = fromTask.Deleted;
            toTask.Completed = DateTime.Parse(fromTask.Completed, CultureInfo.InvariantCulture);
        }
    }
}
