using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleTasksSynchronizer.DataAbstraction.Execution
{
    public class SingleExecutionEnforcementExecutor : ISingleExecutionEnforcementExecutor
    {
        readonly Dictionary<string, HashSet<string>> _executedMap = new Dictionary<string, HashSet<string>>();
        
        public async Task ExecuteOnceAsync(Func<string, Task> func, string param1)
        {
            var functionKey = func.ToString();

            if (!_executedMap.ContainsKey(functionKey))
            {
                _executedMap.Add(functionKey, new HashSet<string>());
            }

            if (!_executedMap[functionKey].Contains(param1))
            {
                await func(param1);

                _executedMap[functionKey].Add(param1);
            }
        }
    }
}
