using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Job.ServicesMonitoring.Core;
using Lykke.Job.ServicesMonitoring.Models;
using Lykke.Job.ServicesMonitoring.Modules;
using Lykke.JobTriggers.Extenstions;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.ServicesMonitoring
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; set; }
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", "ServicesMonitoring API");
            });

            var builder = new ContainerBuilder();
            var settingsManager = Configuration.LoadSettings<AppSettings>("SettingsUrl");
            var appSettings = settingsManager.CurrentValue;

            builder.RegisterInstance(appSettings).AsSelf();

            var log = CreateLogWithSlack(services, settingsManager);

            builder.RegisterModule(new JobModule(appSettings.ServiceMonitoringJob, settingsManager.Nested(a => a.ServiceMonitoringJob), log));

            builder.AddTriggers();

            builder.Populate(services);

            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseLykkeMiddleware("ServicesMonitoring", ex => new ErrorResponse { ErrorMessage = "Technical problem" });

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(x =>
            {
                x.RoutePrefix = "swagger/ui";
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                ApplicationContainer.Dispose();
            });
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settingsManager)
        {
            var logAggregate = new AggregateLogger();

            var logToConsole = new LogToConsole();
            logAggregate.AddLog(logToConsole);

            var dbLogConnectionStringManager = settingsManager.Nested(x => x.ServiceMonitoringJob.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            if (string.IsNullOrEmpty(dbLogConnectionString))
            {
                logToConsole.WriteWarningAsync(nameof(Startup), nameof(CreateLogWithSlack), "Table loggger is not inited").Wait();
                return logAggregate;
            }

            if (dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}"))
                throw new InvalidOperationException($"LogsConnString {dbLogConnectionString} is not filled in settings");

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "ServiceMonitoringLog", logToConsole),
                logToConsole);

            var settings = settingsManager.CurrentValue;

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.SlackNotifications.AzureQueue.QueueName
            }, logAggregate);

            var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, logToConsole);

            // Creating azure storage logger, which logs own messages to concole log
            var azureStorageLogger = new LykkeLogToAzureStorage(
                persistenceManager,
                slackNotificationsManager,
                logToConsole);

            azureStorageLogger.Start();

            logAggregate.AddLog(azureStorageLogger);

            return logAggregate;
        }
    }
}