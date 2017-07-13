using System;

namespace Lykke.Job.ServicesMonitoring.Core.Domain.Monitoring
{
    public interface IMonitoringRecord
    {
        string ServiceName { get; }
        DateTime DateTime { get; }
        string Version { get; }
    }
}
