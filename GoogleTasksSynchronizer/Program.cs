using GoogleTasksSynchronizer.BusinessLogic;
using GoogleTasksSynchronizer.BusinessLogic.Data;
using GoogleTasksSynchronizer.Configuration;
using GoogleTasksSynchronizer.DataAbstraction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddLogging();

        services.AddSingleton<IApplicationStateManager, ApplicationStateManager>();

        services.AddScoped<ITaskChangesProcessor, TaskChangesProcessor>();
        services.AddScoped<IDeletedTasksProcessor, DeletedTasksProcessor>();

        services.AddScoped<IMasterTaskGroupBusinessManager, MasterTaskGroupBusinessManager>();

        services.AddOptions<SynchronizationTargetsOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration.Bind(settings));
        services.AddOptions<GoogleClientSecretOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration.Bind(settings));

        services.AddScoped<ISynchronizationTargetsProvider, SynchronizationTargetsProvider>();
        services.AddScoped<IMasterTaskBusinessManager, MasterTaskBusinessManager>();
        services.AddScoped<IMasterTaskManager, MasterTaskManager>();

        services.AddScoped<ITaskBusinessManager, TaskBusinessManager>();
        services.AddScoped<ITaskManager, TaskManager>();
        services.AddScoped<ITaskServiceFactory, TaskServiceFactory>();

        services.AddScoped<IGoogleClientSecretProvider, GoogleClientSecretProvider>();
        services.AddScoped<IGoogleUserCredentialsManager, GoogleUserCredentialsManager>();

        services.AddScoped<ITaskCreator, TaskCreator>();
        services.AddScoped<ITaskUpdater, TaskUpdater>();
        services.AddScoped<ITaskDeleter, TaskDeleter>();

        services.AddScoped<ITaskMapper, TaskMapper>();

        services.AddScoped<ITaskListLogger, TaskListLogger>();

        services.AddScoped<ITaskSorter, TaskSorter>();
    })
    .Build();

host.Run();
