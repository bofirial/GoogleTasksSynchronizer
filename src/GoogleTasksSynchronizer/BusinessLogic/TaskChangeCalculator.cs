using GoogleTasksSynchronizer.Models;
using Microsoft.Extensions.Logging;
using System;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskChangeCalculator : ITaskChangeCalculator
    {
        private readonly ILogger _logger;
        private readonly ITaskServiceFactory _taskServiceFactory;
        private readonly ITaskBusinessManager _taskBusinessManager;

        public TaskChangeCalculator(
            ILogger logger,
            ITaskServiceFactory taskServiceFactory,
            ITaskBusinessManager taskBusinessManager)
        {
            _logger = logger;
            _taskServiceFactory = taskServiceFactory;
            _taskBusinessManager = taskBusinessManager;
        }

        public void CalculateTaskChanges(TaskAccount taskAccount, TasksSynchronizerState tasksSynchronizerState, TaskChanges taskChanges)
        {
            var taskService = _taskServiceFactory.CreateTaskService(taskAccount);

            var listRequest = taskService.Tasks.List(taskAccount.SynchronizationTarget.TaskListId);

            listRequest.ShowCompleted = true;
            listRequest.ShowDeleted = true;
            listRequest.ShowHidden = true;

            listRequest.UpdatedMin = DateTime.Today.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

            taskAccount.GoogleTasks = _taskBusinessManager.RequestAllGoogleTasks(listRequest);

            _logger.LogInformation($"{taskAccount.GoogleTasks.Count} updated tasks today for {taskAccount.SynchronizationTarget.GoogleAccountName}");

            foreach (var task in taskAccount.GoogleTasks)
            {
                var storedTask = _taskBusinessManager.GetStoredTaskById(task.Id, tasksSynchronizerState);

                if (storedTask != null && _taskBusinessManager.TasksMustBeCleared(task, storedTask))
                {
                    taskChanges.TasksToClear.Add(task);
                }

                if (null == storedTask)
                {
                    if (task.Hidden != true && task.Deleted != true)
                    {
                        taskChanges.TasksToCreate.Add(task);
                    }

                    continue;
                }

                if (!_taskBusinessManager.TasksAreLogicallyEqual(task, storedTask))
                {
                    taskChanges.TasksToModify.Add(task);
                    continue;
                }
            }
        }
    }
}
