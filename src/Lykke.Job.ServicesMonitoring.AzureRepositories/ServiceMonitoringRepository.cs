using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.ServicesMonitoring.Core.Domain.Monitoring;

namespace Lykke.Job.ServicesMonitoring.AzureRepositories
{
    public class ServiceMonitoringRepository : IServiceMonitoringRepository
    {
        readonly INoSQLTableStorage<MonitoringRecordEntity> _tableStorage;

        public ServiceMonitoringRepository(INoSQLTableStorage<MonitoringRecordEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<IMonitoringRecord>> GetAllAsync()
        {
            var partitionKey = MonitoringRecordEntity.GeneratePartitionKey();

            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public Task ScanAllAsync(Func<IEnumerable<IMonitoringRecord>, Task> chunk)
        {
            var partitionKey = MonitoringRecordEntity.GeneratePartitionKey();

            return _tableStorage.ScanDataAsync(partitionKey, chunk);
        }

        public Task UpdateOrCreate(IMonitoringRecord record)
        {
            var entity = MonitoringRecordEntity.Create(record);

            return _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
