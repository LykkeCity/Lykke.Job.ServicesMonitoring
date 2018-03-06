using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Lykke.JobTriggers.Triggers;
using Microsoft.AspNetCore.Hosting;

namespace Lykke.Job.ServicesMonitoring
{
    class Program
    {
        public static string EnvInfo => Environment.GetEnvironmentVariable("ENV_INFO");

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Lykke.Job.ServicesMonitoring version {Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion}");
#if DEBUG
            Console.WriteLine("Is DEBUG");
#else
            Console.WriteLine("Is RELEASE");
#endif
            Console.WriteLine($"ENV_INFO: {EnvInfo}");

            var webHostCancellationTokenSource = new CancellationTokenSource();
            IWebHost webHost = null;
            TriggerHost triggerHost = null;
            Task webHostTask = null;
            Task triggerHostTask = null;
            var end = new ManualResetEvent(false);

            try
            {
                AssemblyLoadContext.Default.Unloading += ctx =>
                {
                    Console.WriteLine("SIGTERM recieved");

                    webHostCancellationTokenSource.Cancel();

                    end.WaitOne();
                };

                webHost = new WebHostBuilder()
                    .UseKestrel()
                    .UseUrls("http://*:5000")
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();

                triggerHost = new TriggerHost(webHost.Services);

                webHostTask = webHost.RunAsync(webHostCancellationTokenSource.Token);
                triggerHostTask = triggerHost.Start();

                // WhenAny to handle any task termination with exception, 
                // or gracefully termination of webHostTask
                Task.WhenAny(webHostTask, triggerHostTask).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error:");
                Console.WriteLine(ex);

                // Lets devops to see startup error in console between restarts in the Kubernetes
                var delay = TimeSpan.FromMinutes(1);

                Console.WriteLine();
                Console.WriteLine($"Process will be terminated in {delay}. Press any key to terminate immediately.");

                await Task.WhenAny(
                            Task.Delay(delay),
                            Task.Run(() =>
                            {
                                Console.ReadKey(true);
                            }));
            }
            finally
            {
                Console.WriteLine("Terminating...");

                webHostCancellationTokenSource.Cancel();
                triggerHost?.Cancel();

                webHostTask?.Wait();
                triggerHostTask?.Wait();

                end.Set();
            }

            Console.WriteLine("Terminated");
        }
    }
}