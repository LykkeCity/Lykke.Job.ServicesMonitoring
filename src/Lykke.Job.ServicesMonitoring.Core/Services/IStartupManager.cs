using System.Threading.Tasks;

namespace Lykke.Job.ServicesMonitoring.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}