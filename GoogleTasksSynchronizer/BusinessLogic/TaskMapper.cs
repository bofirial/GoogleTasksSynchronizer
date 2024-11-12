using System.Globalization;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Newtonsoft.Json;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskMapper : ITaskMapper
    {
        public void MapTask(Google::Task toTask, MasterTask fromTask)
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

        public void MapTask(MasterTask toTask, Google::Task fromTask)
        {
            toTask = toTask ?? throw new ArgumentNullException(nameof(toTask));
            fromTask = fromTask ?? throw new ArgumentNullException(nameof(fromTask));

            toTask.Title = fromTask.Title;
            toTask.Due = fromTask.Due != null ? DateTime.Parse(fromTask.Due, CultureInfo.InvariantCulture) : null;
            toTask.Notes = fromTask.Notes;
            toTask.Status = fromTask.Status;
            toTask.Deleted = fromTask.Deleted;
            toTask.Completed = fromTask.Completed != null ? DateTime.Parse(fromTask.Completed, CultureInfo.InvariantCulture) : null;
        }
    }
}
