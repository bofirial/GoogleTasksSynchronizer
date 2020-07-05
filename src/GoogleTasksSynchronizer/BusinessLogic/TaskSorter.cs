using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using Microsoft.Extensions.Logging;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class TaskSorter : ITaskSorter
    {
        private readonly ILogger<TaskSorter> _logger;
        private readonly ITaskBusinessManager _taskBusinessManager;

        public TaskSorter(ILogger<TaskSorter> logger, ITaskBusinessManager taskBusinessManager)
        {
            _logger = logger;
            _taskBusinessManager = taskBusinessManager;
        }

        public async Task SortTasksAsync(TaskAccountGroup taskAccountGroup)
        {
            taskAccountGroup = taskAccountGroup ?? throw new ArgumentNullException(nameof(taskAccountGroup));

            if (taskAccountGroup.Tasks.Any(t => t.Updated != null && DateTime.Parse(t.Updated, CultureInfo.InvariantCulture) > DateTime.Now.AddMinutes(-1)))
            {
                return;
            }

            var orderedTasks = taskAccountGroup.Tasks.Where(t => t.Completed == null).OrderBy(t => t.GetOrderKey());

            string previousTaskId = null;
            string previousOrderKey = null;
            var previousTaskTitle = string.Empty;
            var previousPosition = string.Empty;

            foreach (var task in orderedTasks)
            {
                if (string.Compare(task.Position, previousPosition, StringComparison.InvariantCulture) < 0 && task.GetOrderKey() != previousOrderKey)
                {
                    _logger.LogInformation($"Sorting task with Title ({task.Title}) in Google Account ({taskAccountGroup.SynchronizationTarget.GoogleAccountName}).");

                    await _taskBusinessManager.MoveAsync(task, taskAccountGroup.SynchronizationTarget, previousTaskId);
                }

                previousTaskId = task.Id;
                previousOrderKey = task.GetOrderKey();
                previousPosition = task.Position;
                previousTaskTitle = task.Title;
            }
        }
    }
}
