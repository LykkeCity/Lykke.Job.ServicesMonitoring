using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.Job.ServicesMonitoring.Core;
using Lykke.Job.ServicesMonitoring.Core.Domain;
using Lykke.Job.ServicesMonitoring.Core.Domain.Monitoring;
using Lykke.Job.ServicesMonitoring.Services;
using Lykke.Job.ServicesMonitoring.Models;

namespace Lykke.Job.ServicesMonitoring.TriggerHandlers
{
    public class CheckAllServices
    {
        private const string SlackMonitorChannel = "Monitor";
        private readonly IServiceMonitoringRepository _serviceMonitoringRepository;
        private readonly HostToCheck[] _hostsToCheck;
        private readonly SlackIntegrationSettings _slackIntegrationSettings;
        private readonly ILog _log;
        private readonly TimeSpan _updateTimeSpan = TimeSpan.FromSeconds(120);

        public CheckAllServices(
            IServiceMonitoringRepository serviceMonitoringRepository,
            AppSettings appSettings,
            ILog log)
        {
            _serviceMonitoringRepository = serviceMonitoringRepository;
            _hostsToCheck = appSettings.ServiceMonitoringJob.HostsToCheck;
            _slackIntegrationSettings = appSettings.SlackIntegration;
            _log = log;
        }

        [TimerTrigger("00:00:30")]
        public async Task CheckServicesUpdates()
        {
            try
            {
                await _serviceMonitoringRepository.ScanAllAsync(HandleRecords);
            }
            catch (Exception ex)
            {
                var msg = $":exclamation: Error while services checking:{Environment.NewLine} {ex.Message}";
                await SendNotification(SlackMonitorChannel, msg);
                throw;
            }
        }

        [TimerTrigger("00:00:30")]
        public async Task VerifyHostsAreOk()
        {
            try
            {
                var amIAliveTasks = _hostsToCheck.Select(CheckHost);

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
                            SlackMonitorChannel, $":exclamation: {host.ServiceName}: {model.Error}");
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
                if (dtUtcNow - record.DateTime > _updateTimeSpan)
                    await SendNotification(
                        SlackMonitorChannel,
                        $":exclamation: No updates from {record.ServiceName} within {(dtUtcNow - record.DateTime).ToString("h'h 'm'm 's's'")}!");
            }
        }

        private async Task SendNotification(string type, string message, string sender = null)
        {
            var webHookUrl = _slackIntegrationSettings.GetChannelWebHook(type);
            if (webHookUrl == null)
                return;

            var text = new StringBuilder();

            if (!string.IsNullOrEmpty(_slackIntegrationSettings.Env))
                text.AppendLine($"Environment: {_slackIntegrationSettings.Env}");

            text.AppendLine(sender != null ? $"{sender} : {message}" : message);

            await HttpRequestClient.PostRequestAsync(new { text = text.ToString() }.ToJson(), webHookUrl);
        }
    }
}
