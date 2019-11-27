using GoogleTasksSynchronizer.Models;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public interface ITaskChangeCalculator
    {
        void CalculateTaskChanges(TaskAccount taskAccount, 
            TasksSynchronizerState tasksSynchronizerState,
            TaskChanges taskChanges);
    }
}
