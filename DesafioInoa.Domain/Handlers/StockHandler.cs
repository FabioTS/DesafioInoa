using System;
using System.IO;
using System.Net;
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
                return new CommandResult(false, "Invalid command", command.Notifications, HttpStatusCode.BadRequest);

            var (commandResult, _) = await _marketDataService.GetStock(command.Symbol);

            return commandResult;
        }

        public async Task<CommandResult> Handle(StockAlertCommand command)
        {
            // Fail fast validation
            if (!command.IsValid())
                return new CommandResult(false, "Invalid command", command.Notifications, HttpStatusCode.BadRequest);

            var (commandResult, stock) = await _marketDataService.GetStock(command.Symbol);

            if (!commandResult.Success) return commandResult;

            string body = null;
            if (stock.Price > command.SellValue)
                body = FormatEmail(LoadEmailTemplate(), command.Symbol, "Sell", stock.Price.ToString(), command.SellValue.ToString());
            else if (stock.Price < command.BuyValue)
                body = FormatEmail(LoadEmailTemplate(), command.Symbol, "Buy", stock.Price.ToString(), command.BuyValue.ToString());

            if (body != null)
                await _mailService.SendMail(command.Email, "Stock Advisor recomendation!", body);

            return commandResult;
        }

        private string FormatEmail(string template, string symbol, string type, string stockPrice, string stockValue) => template
            .Replace("{{Symbol}}", symbol)
            .Replace("{{Buy_or_Sell}}", type)
            .Replace("{{StockPrice}}", stockPrice)
            .Replace("{{StockValue}}", stockValue)
            ;

        private string LoadEmailTemplate()
        {
            try
            {
                string appPath;

                try // Path using dotnet run (dll)
                {
                    appPath = Directory.GetParent(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))).ToString();
                    return File.ReadAllText(Path.Combine(appPath.Replace(".App", ".Domain"), "Templates/") + "email.html");
                }
                catch // Path using self contained app (exe)
                {
                    return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Templates/") + "email.html");
                }
            }
            catch // Fallback text
            {
                return "Your Stock advisor is recomending to {{Buy_or_Sell}} \"{{Symbol}}\". \n\rCurrent price is {{StockPrice}} and reference price is {{StockValue}}";
            }
        }
    }
}
