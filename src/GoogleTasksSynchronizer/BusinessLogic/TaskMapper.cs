using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskMapper : ITaskMapper
    {
        public void MapTask(Task toTask, MasterTask fromTask)
        {
            toTask.Title = fromTask.Title;
            toTask.Due = fromTask.Due;
            toTask.Notes = fromTask.Notes;
            toTask.Status = fromTask.Status;
            toTask.Deleted = fromTask.Deleted;
            toTask.Completed = fromTask.Completed;
            toTask.Hidden = fromTask.Hidden;
        }

        public void MapTask(MasterTask toTask, Task fromTask)
        {
            toTask.Title = fromTask.Title;
            toTask.Due = fromTask.Due;
            toTask.Notes = fromTask.Notes;
            toTask.Status = fromTask.Status;
            toTask.Deleted = fromTask.Deleted;
            toTask.Completed = fromTask.Completed;
            toTask.Hidden = fromTask.Hidden;
        }
    }
}
