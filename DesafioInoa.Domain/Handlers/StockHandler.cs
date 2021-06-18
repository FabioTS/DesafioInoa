using System;
using System.Threading;
using System.Threading.Tasks;
using DesafioInoa.Domain.Commands;
using DesafioInoa.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace DesafioInoa.Domain.Handlers
{
    public class StockHandler :
        IHandler<StockGetCommand>,
        IHandler<StockAlertCommand>
    {
        private readonly IMarketDataService _marketDataService;
        private readonly IMailService _mailService;
        private readonly IConfiguration _settings;

        public StockHandler(IMarketDataService marketDataService, IMailService mailService, IConfiguration settings)
        {
            _marketDataService = marketDataService ?? throw new ArgumentNullException("IMarketDataService");
            _mailService = mailService ?? throw new ArgumentNullException("IMailService");
            _settings = settings ?? throw new ArgumentNullException("IConfiguration");
        }

        public async Task<CommandResult> Handle(StockGetCommand command)
        {
            // Fail fast validation
            if (!command.IsValid())
                return new CommandResult(false, "Invalid command", command.Notifications);

            var stock = await _marketDataService.GetStock(command.Symbol);

            return new CommandResult(true, "Stock retrieved!", stock);
        }

        public async Task<CommandResult> Handle(StockAlertCommand command)
        {
            // Fail fast validation
            if (!command.IsValid())
                return new CommandResult(false, "Invalid command", command.Notifications);

            var toEmail = _settings["MailSettings:ToEmail"];
            var monitoringIntervalMs = int.Parse(_settings["StockQuoteMonitoringIntervalMs"]);

            string key = null;
            do
            {
                var stock = await _marketDataService.GetStock(command.Symbol);
                Console.WriteLine(stock.Price);
                Thread.Sleep(monitoringIntervalMs);
            } while (key == null);

            return new CommandResult(true, "Finished Stock monitoring", null);
        }
    }
}

