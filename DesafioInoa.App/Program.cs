using System.Threading.Tasks;
using DesafioInoa.App.Services;
using DesafioInoa.Domain.Handlers;
using DesafioInoa.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DesafioInoa.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IMarketDataService, HGFinanceService>();
                services.AddSingleton<IMailService, MailSmtpService>();
                services.AddSingleton<StockHandler, StockHandler>();
                services.AddHostedService<StockQuoteAlert>();
            })
            .RunConsoleAsync();
        }
    }
}
