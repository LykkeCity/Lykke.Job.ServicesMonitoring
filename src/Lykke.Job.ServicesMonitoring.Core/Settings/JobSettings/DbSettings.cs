using System.Diagnostics;

namespace Lykke.Job.ServicesMonitoring.Core.Settings.JobSettings
{
    [DebuggerDisplay("LogsConnString: {LogsConnString}")]
    public class DbSettings
    {
        public string LogsConnString { get; set; }

        public string SharedStorageConnString { get; set; }
    }
}
