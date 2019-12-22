using System.Globalization;

using Google = Google.Apis.Tasks.v1.Data;

namespace GoogleTasksSynchronizer.BusinessLogic
{
    public static class TaskExtensions
    {
        public static string GetOrderKey(this Google::Task task) => $"{task?.Due?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "zzzz-zz-zz"}-{task.Title}"; 
    }
}
