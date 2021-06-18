using System;
using System.Threading;
using System.Threading.Tasks;
using DesafioInoa.App.Services;
using DesafioInoa.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DesafioInoa.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<ConsoleHostedService>();
                services.AddSingleton<IMarketDataService, HGFinanceService>();
            })
            .RunConsoleAsync();
        }

        internal sealed class ConsoleHostedService : IHostedService
        {
            private readonly ILogger _logger;
            private readonly IHostApplicationLifetime _appLifetime;
            private readonly IMarketDataService _marketDataService;

            public ConsoleHostedService(
                ILogger<ConsoleHostedService> logger,
                IHostApplicationLifetime appLifetime,
                IMarketDataService marketDataService)
            {
                _logger = logger;
                _appLifetime = appLifetime;
                _marketDataService = marketDataService;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                var commandLineArgs = Environment.GetCommandLineArgs();
                _logger.LogDebug($"Starting with arguments: {string.Join(" ", commandLineArgs)}");
                if(commandLineArgs.Length != 5) {
                    _logger.LogError("Invalid number of arguments, must have 3: [stock, sell, buy]");
                    return Task.CompletedTask;
                }

                _appLifetime.ApplicationStarted.Register(() =>
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            var stock = await _marketDataService.GetStock(Environment.GetCommandLineArgs()[2]);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unhandled exception!");
                        }
                        finally
                        {
                            // Stop the application once the work is done
                            _appLifetime.StopApplication();
                        }
                    });
                });

                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
