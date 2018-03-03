using System.Diagnostics;

namespace Lykke.Job.ServicesMonitoring.Settings.SlackNotifications
{
    [DebuggerDisplay("{QueueName}: {ConnectionString}")]
    public class AzureQueuePublicationSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }
}