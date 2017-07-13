using System.Linq;

namespace Lykke.Job.ServicesMonitoring.Core
{
    public class AppSettings
    {
        public ServiceMonitoringSettings ServiceMonitoringJob { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        public SlackIntegrationSettings SlackIntegration { get; set; }
    }

    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }

        public int ThrottlingLimitSeconds { get; set; }
    }

    public class AzureQueueSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }

    public class HostToCheck
    {
        public string ServiceName { get; set; }
        public string Url { get; set; }
    }

    public class ServiceMonitoringSettings
    {
        public string LogsConnString { get; set; }

        public string SharedStorageConnString { get; set; }

        public HostToCheck[] HostsToCheck { get; set; }
    }

    public class Channel
    {
        public string Type { get; set; }
        public string WebHookUrl { get; set; }
    }

    public class SlackIntegrationSettings
    {
        public string Env { get; set; }
        public Channel[] Channels { get; set; }

        public string GetChannelWebHook(string type)
        {
            return Channels.FirstOrDefault(x => x.Type == type)?.WebHookUrl;
        }
    }
}