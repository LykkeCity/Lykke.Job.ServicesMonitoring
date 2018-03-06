using Autofac;

using AzureStorage.Tables;

using Common.Cache;
using Common.Log;

using Lykke.Job.ServicesMonitoring.AzureRepositories;
using Lykke.Job.ServicesMonitoring.Core.Domain.Monitoring;
using Lykke.Job.ServicesMonitoring.Core.Services;
using Lykke.Job.ServicesMonitoring.Settings.JobSettings;
using Lykke.Job.ServicesMonitoring.Services;
using Lykke.SettingsReader;

namespace Lykke.Job.ServicesMonitoring.Modules
{
    public class JobModule : Module
    {
        private readonly ServiceMonitoringSettings _settings;
        private readonly IReloadingManager<DbSettings> _dbSettingsManager;
        private readonly ILog _log;

        public JobModule(ServiceMonitoringSettings settings, IReloadingManager<DbSettings> dbSettingsManager, ILog log)
        {
            _settings = settings;
            _dbSettingsManager = dbSettingsManager;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // NOTE: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            // builder.RegisterType<QuotesPublisher>()
            //  .As<IQuotesPublisher>()
            //  .WithParameter(TypedParameter.From(_settings.Rabbit.ConnectionString))
            builder.RegisterInstance(_settings)
                .SingleInstance();

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            // NOTE: You can implement your own poison queue notifier. See https://github.com/LykkeCity/JobTriggers/blob/master/readme.md
            // builder.Register<PoisionQueueNotifierImplementation>().As<IPoisionQueueNotifier>();

            var cacheManager = new MemoryCacheManager();
            builder.RegisterInstance(cacheManager).As<ICacheManager>();

            RegisterAzureRepositories(builder);
        }

        private void RegisterAzureRepositories(ContainerBuilder builder)
        {
            builder.RegisterInstance<IServiceMonitoringRepository>(
                new ServiceMonitoringRepository(
                    AzureTableStorage<MonitoringRecordEntity>.Create(
                        _dbSettingsManager.Nested(x => x.SharedStorageConnString), "Monitoring", _log)));
        }
    }
}