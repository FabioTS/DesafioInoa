using System.Threading.Tasks;
using DesafioInoa.Domain.ValueObjects;

namespace DesafioInoa.Domain.Services
{
    public interface IMarketDataService
    {
        Task<Stock> GetStock(string symbol);
    }
}