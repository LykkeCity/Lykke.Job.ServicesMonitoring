using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.PlatformAbstractions;

using Common;
using Common.Log;

using Lykke.Job.ServicesMonitoring.Core.Domain.Monitoring;
using Lykke.Job.ServicesMonitoring.Settings;
using Lykke.Job.ServicesMonitoring.Settings.JobSettings;
using Lykke.Job.ServicesMonitoring.Settings.SlackNotifications;
using Lykke.Job.ServicesMonitoring.Models;
using Lykke.Job.ServicesMonitoring.Services;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.SettingsReader;

namespace Lykke.Job.ServicesMonitoring.TriggerHandlers
{
    public class CheckAllServices
    {
        private const string SlackMonitorChannel = "Monitor";

        private readonly IServiceMonitoringRepository _serviceMonitoringRepository;
        private readonly IReloadingManager<ServiceMonitoringSettings> _serviceMonitoringSettings;
        private readonly IReloadingManager<SlackIntegrationSettings> _slackIntegrationSettings;
        private readonly ILog _log;
        private readonly TimeSpan _updateTimeSpan = TimeSpan.FromSeconds(120);
        private readonly string _appName = PlatformServices.Default.Application.ApplicationName;
        private readonly string _appVersion = PlatformServices.Default.Application.ApplicationVersion;

        public CheckAllServices(
            IServiceMonitoringRepository serviceMonitoringRepository,
            IReloadingManager<AppSettings> settings,
            ILog log)
        {
            _serviceMonitoringRepository = serviceMonitoringRepository;
            _serviceMonitoringSettings = settings.Nested(x => x.ServiceMonitoringJob);
            _slackIntegrationSettings = settings.Nested(x => x.SlackIntegration);
            _log = log;
        }

        [TimerTrigger("00:01:00")]
        public async Task CheckServicesUpdates()
        {
            try
            {
                await _serviceMonitoringRepository.ScanAllAsync(HandleRecords);
            }
            catch (Exception ex)
            {
                var msg = $":exclamation: {_appName} {_appVersion}: Error while services checking:{Environment.NewLine} {ex.Message}";
                await SendNotification(SlackMonitorChannel, msg);
                throw;
            }
        }

        [TimerTrigger("00:01:00")]
        public async Task VerifyHostsAreOk()
        {
            try
            {
                var amIAliveTasks = _serviceMonitoringSettings.CurrentValue.HostsToCheck.Select(CheckHost);

                await Task.WhenAny(Task.WhenAll(amIAliveTasks), Task.Delay(10000));
            }
            catch (Exception)
            {
                await SendNotification(SlackMonitorChannel, "Error while checking hosts!");
                throw;
            }
        }

        private async Task CheckHost(HostToCheck host)
        {
            try
            {
                var result = await HttpRequestClient.GetRequestAsync(host.Url);

                string version = null;
                try
                {
                    var model = result.DeserializeJson<AmIAliveModel>();
                    version = model?.Version;

                    if (!string.IsNullOrEmpty(model?.Error))
                    {
                        await SendNotification(
                            SlackMonitorChannel, $":exclamation: {_appName} {_appVersion}: {host.ServiceName}: {model.Error}");
                    }
                }
                catch
                {
                    // ignored
                    //not all system support version yet
                }

                await _serviceMonitoringRepository.UpdateOrCreate(
                    MonitoringRecord.Create(host.ServiceName, DateTime.UtcNow, version));
            }
            catch (WebException ex)
            {
                using (var receiveStream = ex.Response.GetResponseStream())
                {
                    using (var sr = new StreamReader(receiveStream))
                    {
                        var content = await sr.ReadToEndAsync();
                        await _log.WriteErrorAsync("CheckAllServices", "CheckHost", content, ex);
                    }
                }
            }
        }

        private async Task HandleRecords(IEnumerable<IMonitoringRecord> records)
        {
            var dtUtcNow = DateTime.UtcNow;
            foreach (var record in records)
            {
                var timeDiff = dtUtcNow - record.DateTime;
                if (timeDiff <= _updateTimeSpan)
                    continue;

                await SendNotification(
                    SlackMonitorChannel,
                    $":exclamation: {_appName} {_appVersion}: No updates from {record.ServiceName} within {timeDiff.ToString("d'd 'h'h 'm'm 's's'")}!");
            }
        }

        private async Task SendNotification(string type, string message, string sender = null)
        {
            var webHookUrl = _slackIntegrationSettings.CurrentValue.GetChannelWebHook(type);
            if (webHookUrl == null)
                return;

            var text = new StringBuilder();

            if (!string.IsNullOrEmpty(_slackIntegrationSettings.CurrentValue.Env))
                text.AppendLine($"Environment: {_slackIntegrationSettings.CurrentValue.Env}");

            text.AppendLine(sender != null ? $"{sender} : {message}" : message);

            await HttpRequestClient.PostRequestAsync(webHookUrl, new { text = text.ToString() }.ToJson());
        }
    }
}
