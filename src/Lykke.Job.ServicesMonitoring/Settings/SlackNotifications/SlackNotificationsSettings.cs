using System.Diagnostics;

namespace Lykke.Job.ServicesMonitoring.Settings.SlackNotifications
{
    [DebuggerDisplay("ThrottlingLimit: {ThrottlingLimitSeconds} seconds")]
    public class SlackNotificationsSettings
    {
        public AzureQueuePublicationSettings AzureQueue { get; set; }
        public int ThrottlingLimitSeconds { get; set; }
    }
}