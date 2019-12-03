using Google.Apis.Tasks.v1.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskMapper : ITaskMapper
    {
        public void MapTask(MasterTask fromTask, Task toTask)
        {
            toTask.Title = fromTask.Title;
            toTask.Due = fromTask.Due;
            toTask.Notes = fromTask.Notes;
            toTask.Status = fromTask.Status;
            toTask.Deleted = fromTask.Deleted;
            toTask.Completed = fromTask.Completed;
            toTask.Hidden = fromTask.Hidden;
        }

        public void MapTask(Task fromTask, MasterTask toTask)
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
