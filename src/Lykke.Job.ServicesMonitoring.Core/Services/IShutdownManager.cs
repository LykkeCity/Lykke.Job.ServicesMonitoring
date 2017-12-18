using System.Threading.Tasks;

namespace Lykke.Job.ServicesMonitoring.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}