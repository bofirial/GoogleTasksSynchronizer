using GoogleTasksSynchronizer.BusinessLogic;
using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(GoogleTasksSynchronizer.Startup))]

namespace GoogleTasksSynchronizer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddSingleton<IApplicationStateManager, ApplicationStateManager>();

            builder.Services.AddScoped<ITaskChangesProcessor, TaskChangesProcessor>();

            builder.Services.AddScoped<IMasterTaskGroupBusinessManager, MasterTaskGroupBusinessManager>();

            builder.Services.AddOptions<SynchronizationTargetsOptions>()
                .Configure<IConfiguration>((settings, configuration) => { configuration.Bind(settings); });
            builder.Services.AddScoped<ISynchronizationTargetsProvider, SynchronizationTargetsProvider>();
            builder.Services.AddScoped<IMasterTaskBusinessManager, MasterTaskBusinessManager>();
            builder.Services.AddScoped<IMasterTaskManager, MasterTaskManager>();

            builder.Services.AddScoped<ITaskBusinessManager, TaskBusinessManager>();
            builder.Services.AddScoped<ITaskManager, TaskManager>();
            builder.Services.AddScoped<ITaskServiceFactory, TaskServiceFactory>();

            builder.Services.AddScoped<IGoogleClientSecretProvider, GoogleClientSecretProvider>();
            builder.Services.AddScoped<IGoogleUserCredentialsManager, GoogleUserCredentialsManager>();

            //builder.Services.AddScoped<ITasksSynchronizerStateManager, TasksSynchronizerStateManager>();
            //builder.Services.AddScoped<ITaskBusinessManager, TaskBusinessManager>();
            //builder.Services.AddScoped<IGoogleTaskAccountManager, GoogleTaskAccountManager>();

            builder.Services.AddScoped<ITaskListLogger, TaskListLogger>();
        }
    }
}
