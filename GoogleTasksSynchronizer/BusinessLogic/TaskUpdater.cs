using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;
using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskUpdater(ITaskMapper taskMapper, ITaskBusinessManager taskBusinessManager, ILogger<TaskUpdater> logger) : ITaskUpdater
    {
        public async Task UpdateTaskAsync(Google::Task task, MasterTask masterTask, List<TaskAccountGroup> taskAccountGroups)
        {
            masterTask = masterTask ?? throw new ArgumentNullException(nameof(masterTask));
            taskAccountGroups = taskAccountGroups ?? throw new ArgumentNullException(nameof(taskAccountGroups));

            taskMapper.MapTask(masterTask, task);
            masterTask.UpdatedOn = DateTime.Now;

            foreach (var taskAccountGroup in taskAccountGroups)
            {
                await UpdateTaskAccountGroupTaskAsync(masterTask, taskAccountGroup);
            }
        }

        private async Task UpdateTaskAccountGroupTaskAsync(MasterTask masterTask, TaskAccountGroup taskAccountGroup)
        {
            var taskToUpdateId = masterTask.TaskMaps.FirstOrDefault(tm =>
                tm.SynchronizationTarget.GoogleAccountName == taskAccountGroup.SynchronizationTarget.GoogleAccountName)?.TaskId;

            var taskToUpdate = taskAccountGroup.Tasks.Where(t => t.Id == taskToUpdateId).FirstOrDefault();

            if (null == taskToUpdate)
            {
                logger.LogError($"Missing Task Mapping for Task: \"{masterTask.Title}\"");
            }
            else if (!taskBusinessManager.TasksAreEqual(masterTask, taskToUpdate))
            {
                taskMapper.MapTask(taskToUpdate, masterTask);

                await taskBusinessManager.UpdateAsync(taskToUpdate, taskAccountGroup.SynchronizationTarget);
            }
        }
    }
}
