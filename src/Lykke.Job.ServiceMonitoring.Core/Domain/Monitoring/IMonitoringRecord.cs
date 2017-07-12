using System;

namespace Lykke.Job.ServiceMonitoring.Core.Domain.Monitoring
{
    public interface IMonitoringRecord
    {
        string ServiceName { get; }
        DateTime DateTime { get; }
        string Version { get; }
    }
}
