using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.ServicesMonitoring.Core.Domain.Monitoring
{
    public interface IServiceMonitoringRepository
    {
        Task<IEnumerable<IMonitoringRecord>> GetAllAsync();
        Task ScanAllAsync(Func<IEnumerable<IMonitoringRecord>, Task> chunk);
        Task UpdateOrCreate(IMonitoringRecord record);
    }
}
