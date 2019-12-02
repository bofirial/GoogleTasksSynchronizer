using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class MasterTaskGroupBusinessManager : IMasterTaskGroupBusinessManager
    {
        private readonly ISynchronizationTargetsProvider _synchronizationTargetManager;
        private readonly IMasterTaskBusinessManager _masterTaskBusinessManager;
        private readonly ITaskBusinessManager _taskBusinessManager;

        public MasterTaskGroupBusinessManager(
            ISynchronizationTargetsProvider synchronizationTargetManager,
            IMasterTaskBusinessManager masterTaskBusinessManager,
            ITaskBusinessManager taskBusinessManager
            )
        {
            _synchronizationTargetManager = synchronizationTargetManager;
            _masterTaskBusinessManager = masterTaskBusinessManager;
            _taskBusinessManager = taskBusinessManager;
        }

        public async Task<List<MasterTaskGroup>> SelectAsync()
        {
            var masterTaskGroups = new List<MasterTaskGroup>();

            var synchronizationTargets = await _synchronizationTargetManager.GetAsync();

            foreach (var synchronizationTarget in synchronizationTargets)
            {
                if (MasterTaskGroupExists(masterTaskGroups, synchronizationTarget.SynchronizationId))
                {
                    await AddTasksToMasterTaskGroup(masterTaskGroups, synchronizationTarget);
                }
                else
                {
                    masterTaskGroups.Add(await CreateMasterTaskGroup(synchronizationTarget));
                }
            }

            return masterTaskGroups;
        }

        private static bool MasterTaskGroupExists(List<MasterTaskGroup> masterTaskGroups, string synchronizationId)
        {
            return masterTaskGroups.Any(m => m.SynchronizationId == synchronizationId);
        }

        private async Task<MasterTaskGroup> CreateMasterTaskGroup(SynchronizationTarget synchronizationTarget)
        {
            return new MasterTaskGroup()
            {
                SynchronizationId = synchronizationTarget.SynchronizationId,
                MasterTasks = await _masterTaskBusinessManager.SelectAllAsync(),
                TaskAccountGroups = new List<TaskAccountGroup>()
                {
                    await CreateTaskAccountGroup(synchronizationTarget)
                }
            };
        }

        private async Task AddTasksToMasterTaskGroup(List<MasterTaskGroup> masterTaskGroups, SynchronizationTarget synchronizationTarget)
        {
            var masterTaskGroup = masterTaskGroups.First(m => m.SynchronizationId == synchronizationTarget.SynchronizationId);

            masterTaskGroup.TaskAccountGroups.Add(await CreateTaskAccountGroup(synchronizationTarget));
        }

        private async Task<TaskAccountGroup> CreateTaskAccountGroup(SynchronizationTarget synchronizationTarget)
        {
            return new TaskAccountGroup()
            {
                GoogleAccountName = synchronizationTarget.GoogleAccountName,
                TaskListId = synchronizationTarget.TaskListId,
                Tasks = await _taskBusinessManager.SelectAllAsync(synchronizationTarget)
            };
        }
    }
}
