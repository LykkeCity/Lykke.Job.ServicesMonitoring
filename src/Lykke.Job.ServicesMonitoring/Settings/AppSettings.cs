using Lykke.Job.ServicesMonitoring.Settings.JobSettings;
using Lykke.Job.ServicesMonitoring.Settings.SlackNotifications;

namespace Lykke.Job.ServicesMonitoring.Settings
{
    public class AppSettings
    {
        public ServiceMonitoringSettings ServiceMonitoringJob { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        public SlackIntegrationSettings SlackIntegration { get; set; }
    }

}