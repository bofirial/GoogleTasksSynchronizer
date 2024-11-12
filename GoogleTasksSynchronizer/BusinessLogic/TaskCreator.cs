using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;
using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskCreator(ITaskMapper taskMapper, ITaskBusinessManager taskBusinessManager, ILogger<TaskCreator> logger) : ITaskCreator
    {
        public async Task<MasterTask> CreateNewTaskAsync(Google::Task taskToCreate, List<TaskAccountGroup> taskAccountGroups)
        {
            taskAccountGroups = taskAccountGroups ?? throw new ArgumentNullException(nameof(taskAccountGroups));

            var masterTask = new MasterTask()
            {
                MasterTaskId = Guid.NewGuid().ToString(),
                TaskMaps = [],
                UpdatedOn = DateTime.Now
            };

            taskMapper.MapTask(masterTask, taskToCreate);

            foreach (var taskAccountGroup in taskAccountGroups)
            {
                var equalTasks = taskAccountGroup.Tasks.Where(t => taskBusinessManager.TasksAreEqual(masterTask, t));

                if (equalTasks.Count() > 1)
                {
                    logger.LogWarning($"Multiple equivalent tasks named \"{masterTask.Title}\" found in google account: {taskAccountGroup.SynchronizationTarget.GoogleAccountName}");
                }

                var task = equalTasks.FirstOrDefault();

                if (null == task)
                {
                    var newTask = new Google::Task();

                    taskMapper.MapTask(newTask, masterTask);

                    task = await taskBusinessManager.InsertAsync(newTask, taskAccountGroup.SynchronizationTarget);
                }

                masterTask.TaskMaps.Add(new TaskMap()
                {
                    SynchronizationTarget = taskAccountGroup.SynchronizationTarget,
                    TaskId = task.Id
                });
            }

            return masterTask;
        }
    }
}
