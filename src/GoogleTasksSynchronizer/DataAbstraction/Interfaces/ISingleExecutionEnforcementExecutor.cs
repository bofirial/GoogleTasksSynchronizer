using System;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction
{
    public interface ISingleExecutionEnforcementExecutor
    {
        Task ExecuteOnceAsync(Func<string, Task> func, string param1); 
    }
}
