using GoogleTasksSynchronizer.DataAbstraction.Models;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskMapper
    {
        void MapTask(Google::Task toTask, MasterTask fromTask);

        void MapTask(MasterTask toTask, Google::Task fromTask);
    }
}
