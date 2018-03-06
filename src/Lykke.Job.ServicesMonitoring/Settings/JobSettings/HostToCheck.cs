using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Lykke.Job.ServicesMonitoring.Settings.JobSettings
{
    [DebuggerDisplay("{ServiceName}: {Url}")]
    public class HostToCheck
    {
        public string ServiceName { get; set; }
        public string Url { get; set; }
    }
}
