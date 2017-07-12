using System;

namespace Lykke.Job.ServiceMonitoring.Core.Domain.Monitoring
{
    public class MonitoringRecord : IMonitoringRecord
    {
        public string ServiceName { get; set; }
        public DateTime DateTime { get; set; }
        public string Version { get; set; }

        public static MonitoringRecord Create(string serviceName, DateTime dateTime, string version)
        {
            return new MonitoringRecord
            {
                ServiceName = serviceName,
                DateTime = dateTime,
                Version = version
            };
        }
    }
}
