using System;
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

        public StockHandler(IMarketDataService marketDataService, IMailService mailService, IConfiguration settings)
        {
            _marketDataService = marketDataService ?? throw new ArgumentNullException("IMarketDataService");
            _mailService = mailService ?? throw new ArgumentNullException("IMailService");
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

            var (commandResult, stock) = await _marketDataService.GetStock(command.Symbol);

            if(!commandResult.Success) return commandResult;

            // TODO, rule to send mail
        
            return commandResult;
        }
    }
}

