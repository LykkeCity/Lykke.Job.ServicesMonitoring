using Lykke.Job.ServicesMonitoring.Core.Settings.JobSettings;
using Lykke.Job.ServicesMonitoring.Core.Settings.SlackNotifications;

namespace Lykke.Job.ServicesMonitoring.Core.Settings
{
    public class AppSettings
    {
        public ServiceMonitoringSettings ServiceMonitoringJob { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        public SlackIntegrationSettings SlackIntegration { get; set; }
    }

}