using System.Diagnostics;

namespace Lykke.Job.ServicesMonitoring.Settings.JobSettings
{
    [DebuggerDisplay("HostsToCheck: {HostsToCheck.Length}")]
    public class ServiceMonitoringSettings
    {
        public DbSettings Db { get; set; }

        public HostToCheck[] HostsToCheck { get; set; }
    }
}