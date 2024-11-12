using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public class MasterTaskGroupBusinessManager(
        ISynchronizationTargetsProvider synchronizationTargetManager,
        IMasterTaskBusinessManager masterTaskBusinessManager,
        ITaskBusinessManager taskBusinessManager
            ) : IMasterTaskGroupBusinessManager
    {
        public async Task<List<MasterTaskGroup>> SelectAsync()
        {
            var masterTaskGroups = new List<MasterTaskGroup>();

            var synchronizationTargets = await synchronizationTargetManager.GetAsync();

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
                MasterTasks = await masterTaskBusinessManager.SelectAllAsync(synchronizationTarget.SynchronizationId),
                TaskAccountGroups =
                [
                    await CreateTaskAccountGroup(synchronizationTarget)
                ]
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
                SynchronizationTarget = synchronizationTarget,
                Tasks = await taskBusinessManager.SelectAllAsync(synchronizationTarget)
            };
        }
    }
}
