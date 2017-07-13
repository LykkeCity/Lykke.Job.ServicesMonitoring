using System;
using Microsoft.WindowsAzure.Storage.Table;
using Lykke.Job.ServicesMonitoring.Core.Domain.Monitoring;

namespace Lykke.Job.ServicesMonitoring.AzureRepositories
{
    public class MonitoringRecordEntity : TableEntity, IMonitoringRecord
    {
        public string ServiceName => RowKey;
        public DateTime DateTime { get; set; }
        public string Version { get; set; }

        public static string GeneratePartitionKey()
        {
            return "Monitoring";
        }

        public static string GenerateRowKey(string serviceName)
        {
            return serviceName;
        }

        public static MonitoringRecordEntity Create(IMonitoringRecord record)
        {
            return new MonitoringRecordEntity
            {
                RowKey = GenerateRowKey(record.ServiceName),
                DateTime = record.DateTime,
                PartitionKey = GeneratePartitionKey(),
                Version = record.Version
            };
        }
    }
}
