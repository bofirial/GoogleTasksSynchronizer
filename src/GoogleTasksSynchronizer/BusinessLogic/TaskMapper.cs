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
            toTask.Due = fromTask.Due != null ? JsonConvert.SerializeObject(fromTask.Due).Trim('"') : null;
            toTask.Notes = fromTask.Notes;
            toTask.Status = fromTask.Status;
            toTask.Deleted = fromTask.Deleted;
            toTask.Completed = fromTask.Completed != null ? JsonConvert.SerializeObject(fromTask.Completed).Trim('"') : null;
        }

        public void MapTask(MasterTask toTask, Task fromTask)
        {
            toTask = toTask ?? throw new ArgumentNullException(nameof(toTask));
            fromTask = fromTask ?? throw new ArgumentNullException(nameof(fromTask));

            toTask.Title = fromTask.Title;
            toTask.Due = (fromTask.Due != null ? (DateTime?)DateTime.Parse(fromTask.Due, CultureInfo.InvariantCulture) : null);
            toTask.Notes = fromTask.Notes;
            toTask.Status = fromTask.Status;
            toTask.Deleted = fromTask.Deleted;
            toTask.Completed = (fromTask.Completed != null ? (DateTime?)DateTime.Parse(fromTask.Completed, CultureInfo.InvariantCulture) : null);
        }
    }
}
