
using GoogleTasksSynchronizer.BusinessLogic;
using GoogleTasksSynchronizer.DataAbstraction;
using GoogleTasksSynchronizer.Google;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(GoogleTasksSynchronizer.Startup))]

namespace GoogleTasksSynchronizer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddScoped<ITasksSynchronizerStateManager, TasksSynchronizerStateManager>();
            builder.Services.AddScoped<ITaskBusinessManager, TaskBusinessManager>();
            builder.Services.AddScoped<IGoogleTaskAccountManager, GoogleTaskAccountManager>();
        }
    }
}
