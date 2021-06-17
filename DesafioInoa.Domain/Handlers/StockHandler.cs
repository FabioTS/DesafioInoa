using System;
using System.Threading;
using System.Threading.Tasks;
using DesafioInoa.Domain.Commands;
using DesafioInoa.Domain.Services;

namespace DesafioInoa.Domain.Handlers
{
    public class StockHandler :
        IHandler<StockGetCommand>,
        IHandler<StockAlertCommand>
    {
        private readonly IMarketDataService _marketDataService;
        private const int MonitoringIntervalMs = 5000;

        public StockHandler(IMarketDataService marketDataService)
        {
            _marketDataService = marketDataService ?? throw new ArgumentNullException("IMarketDataService");
        }

        public async Task<ICommandResult> Handle(StockGetCommand command)
        {
            // Fail fast validation
            if (!command.IsValid())
                return new CommandResult(false, "Invalid command", command.Notifications);

            var stock = await _marketDataService.GetStock(command.Symbol);

            return new CommandResult(true, "Stock retrieved!", stock);
        }

        public async Task<ICommandResult> Handle(StockAlertCommand command)
        {
            // Fail fast validation
            if (!command.IsValid())
                return new CommandResult(false, "Invalid command", command.Notifications);

            string key = null;
            Console.WriteLine("Press any key to stop monitoring");
            do
            {
                var stock = await _marketDataService.GetStock(command.Symbol);
                Console.WriteLine(stock.Price);
                Thread.Sleep(MonitoringIntervalMs);
                key = Console.ReadLine();
            } while (key == null);

            return new CommandResult(true, "Finished Stock monitoring", null);
        }
    }
}

