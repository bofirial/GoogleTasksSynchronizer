using GoogleTasksSynchronizer.DataAbstraction.Models;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskMapper
    {
        void MapTask(MasterTask fromTask, Google::Task toTask);

        void MapTask(Google::Task fromTask, MasterTask toTask);
    }
}
