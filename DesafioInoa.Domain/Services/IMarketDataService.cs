using System.Threading.Tasks;
using DesafioInoa.Domain.Commands;
using DesafioInoa.Domain.ValueObjects;

namespace DesafioInoa.Domain.Services
{
    public interface IMarketDataService
    {
        Task<(CommandResult, Stock)> GetStock(string symbol);
    }
}