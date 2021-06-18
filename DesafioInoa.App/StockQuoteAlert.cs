using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DesafioInoa.Domain.Commands;
using DesafioInoa.Domain.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DesafioInoa.App
{
    class StockQuoteAlert : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly StockHandler _handler;

        public StockQuoteAlert(
            ILogger<StockQuoteAlert> logger,
            IHostApplicationLifetime appLifetime,
            StockHandler handler)
        {
            _logger = logger ?? throw new ArgumentNullException("ILogger");
            _appLifetime = appLifetime ?? throw new ArgumentNullException("IHostApplicationLifetime");
            _handler = handler ?? throw new ArgumentNullException("StockHandler");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            _logger.LogDebug($"Starting with arguments: {string.Join(" ", commandLineArgs)}");

            if (commandLineArgs.Length != 4)
            {
                _logger.LogError("Invalid number of arguments, must have 3: [stock symbol, sell value, buy value]");
                return Task.CompletedTask;
            }

            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var command = new StockAlertCommand(commandLineArgs[1], int.Parse(commandLineArgs[2]), int.Parse(commandLineArgs[3]));
                        var commandResult = await _handler.Handle(command);
                        _logger.LogInformation(JsonSerializer.Serialize(commandResult, new JsonSerializerOptions() { WriteIndented = true }));
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
