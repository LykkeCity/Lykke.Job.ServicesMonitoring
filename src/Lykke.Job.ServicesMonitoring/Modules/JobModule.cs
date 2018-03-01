using Autofac;
using Common.Cache;
using Common.Log;
using AzureStorage.Tables;
using Lykke.SettingsReader;
using Lykke.Job.ServicesMonitoring.Core;
using Lykke.Job.ServicesMonitoring.Core.Domain.Monitoring;
using Lykke.Job.ServicesMonitoring.AzureRepositories;

namespace Lykke.Job.ServicesMonitoring.Modules
{
    public class JobModule : Module
    {
        private readonly ServiceMonitoringSettings _settings;
        private readonly IReloadingManager<ServiceMonitoringSettings> _settingsManager;
        private readonly ILog _log;

        public JobModule(ServiceMonitoringSettings settings, IReloadingManager<ServiceMonitoringSettings> settingsManager, ILog log)
        {
            _settings = settings;
            _settingsManager = settingsManager;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings)
                .SingleInstance();

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            var cacheManager = new MemoryCacheManager();
            builder.RegisterInstance(cacheManager).As<ICacheManager>();

            RegisterAzureRepositories(builder);
        }

        private void RegisterAzureRepositories(ContainerBuilder builder)
        {
            builder.RegisterInstance<IServiceMonitoringRepository>(
                new ServiceMonitoringRepository(
                    AzureTableStorage<MonitoringRecordEntity>.Create(_settingsManager.ConnectionString(s => s.SharedStorageConnString), "Monitoring", _log)));
        }
    }
}