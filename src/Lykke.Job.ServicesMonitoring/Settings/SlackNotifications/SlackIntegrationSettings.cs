using System.Diagnostics;
using System.Linq;

namespace Lykke.Job.ServicesMonitoring.Settings.SlackNotifications
{
    [DebuggerDisplay("{Env} env: {Channels.Length} channels")]
    public class SlackIntegrationSettings
    {
        public string Env { get; set; }
        public Channel[] Channels { get; set; }

        public string GetChannelWebHook(string type)
        {
            return Channels.FirstOrDefault(x => x.Type == type)?.WebHookUrl;
        }
    }

    [DebuggerDisplay("{Type}: {WebHookUrl}")]
    public class Channel
    {
        public string Type { get; set; }
        public string WebHookUrl { get; set; }
    }
}
