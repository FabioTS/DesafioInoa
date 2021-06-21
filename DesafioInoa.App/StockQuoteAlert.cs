using System;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DesafioInoa.Domain.Commands;
using DesafioInoa.Domain.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DesafioInoa.App
{
    public class StockQuoteAlert : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly StockHandler _handler;
        private readonly int _monitoringIntervalMs;
        private readonly string _alertEmail;
        public const string ARGS_ENV_VAR = "STOCK_MONITOR_ARGS";

        public StockQuoteAlert(
            ILogger<StockQuoteAlert> logger,
            IHostApplicationLifetime appLifetime,
            StockHandler handler,
            IConfiguration settings)
        {
            _logger = logger ?? throw new ArgumentNullException("ILogger");
            _appLifetime = appLifetime ?? throw new ArgumentNullException("IHostApplicationLifetime");
            _handler = handler ?? throw new ArgumentNullException("StockHandler");
            _monitoringIntervalMs = int.Parse(settings["StockQuoteMonitoringIntervalMs"] ?? throw new ArgumentNullException("StockQuoteMonitoringIntervalMs"));
            _alertEmail = settings["MailSettings:ToEmail"] ?? throw new ArgumentNullException("MailSettings:ToEmail");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var commandLineArgs = Environment.GetCommandLineArgs();
                        var envArgs = Environment.GetEnvironmentVariable(ARGS_ENV_VAR);

                        // Try to get from environment variable
                        if (commandLineArgs.Length == 1 && envArgs != default)
                            commandLineArgs = envArgs.Split();

                        _logger.LogDebug($"Starting with arguments: {string.Join(" ", commandLineArgs)}");

                        if (commandLineArgs.Length != 4)
                        {
                            _logger.LogError("Invalid number of arguments, must have 3: [stock symbol, sell value, buy value]");
                            _appLifetime.StopApplication();
                            return;
                        }

                        CommandResult commandResult;
                        var command = new StockAlertCommand(
                            commandLineArgs[1],
                            double.Parse(commandLineArgs[2].Replace(',', '.'), CultureInfo.InvariantCulture),
                            double.Parse(commandLineArgs[3].Replace(',', '.'), CultureInfo.InvariantCulture),
                            _alertEmail);

                        do
                        {
                            commandResult = await _handler.Handle(command);
                            _logger.LogInformation(JsonSerializer.Serialize(commandResult, new JsonSerializerOptions() { WriteIndented = true }));
                            if (!commandResult.Success) break;
                            Thread.Sleep(_monitoringIntervalMs);

                        } while (!cancellationToken.IsCancellationRequested);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception!");
                    }
                    finally
                    {
                        _logger.LogDebug($"StockQuoteAlert Task {Task.CurrentId} finished.");
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
