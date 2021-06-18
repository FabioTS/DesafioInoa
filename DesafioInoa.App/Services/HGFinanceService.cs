using System;
using System.Net.Http;
using System.Threading.Tasks;
using DesafioInoa.Domain.Services;
using DesafioInoa.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DesafioInoa.App.Services
{
    public class HGFinanceService : IMarketDataService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _settings;
        private readonly HttpClient _httpClient;
        private readonly string _providerKey;


        public HGFinanceService(ILogger<HGFinanceService> logger, IConfiguration settings)
        {
            _logger = logger ?? throw new ArgumentNullException("ILogger");
            _settings = settings ?? throw new ArgumentNullException("IConfiguration");
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_settings.GetValue<string>("HGFinance:BaseUrl"));
            _providerKey = _settings.GetValue<string>("HGFinance:Key");
        }

        public async Task<Stock> GetStock(string symbol)
        {
            var qs = $"?key={_providerKey}&symbol={symbol}";
            var response = await _httpClient.GetAsync("/finance/stock_price" + qs);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("An error ocurred while trying to fetch Stock");
                return default;
            }

            _logger.LogInformation(responseContent);

            return new Stock();
        }
    }
}