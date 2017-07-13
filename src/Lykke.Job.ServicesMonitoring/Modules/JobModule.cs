using Autofac;
using Common.Cache;
using Common.Log;
using AzureStorage.Tables;
using Lykke.Job.ServicesMonitoring.Core;
using Lykke.Job.ServicesMonitoring.Core.Domain.Monitoring;
using Lykke.Job.ServicesMonitoring.AzureRepositories;

namespace Lykke.Job.ServicesMonitoring.Modules
{
    public class JobModule : Module
    {
        private readonly ServiceMonitoringSettings _settings;
        private readonly ILog _log;

        public JobModule(ServiceMonitoringSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings)
                .SingleInstance();

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

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
                    new AzureTableStorage<MonitoringRecordEntity>(_settings.SharedStorageConnString, "Monitoring", _log)));
        }
    }
}