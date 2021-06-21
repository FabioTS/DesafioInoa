using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DesafioInoa.Domain.Commands;
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

        public async Task<(CommandResult, Stock)> GetStock(string symbol)
        {
            var qs = $"?key={_providerKey}&symbol={symbol}";
            var response = await _httpClient.GetAsync("/finance/stock_price" + qs);
            var jsonResponse = await JsonSerializer.DeserializeAsync<JsonElement>(response.Content.ReadAsStream());

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("An error ocurred while trying to fetch Stock");
                return (new CommandResult(false, "An error ocurred while trying to fetch Stock", null, HttpStatusCode.BadGateway), default);
            }

            var jResults = jsonResponse.GetProperty("results");
            if (!jResults.TryGetProperty(symbol.ToUpperInvariant(), out var jSymbol))
            {
                var msg = $"Error to get Stock for #{symbol}: Símbolo não disponível";
                _logger.LogError(msg);
                return (new CommandResult(false, msg, null, HttpStatusCode.BadRequest), default);
            }

            if (jSymbol.TryGetProperty("error", out var jError))
            {
                var msg = jSymbol.GetProperty("message").GetString();
                _logger.LogError(msg);
                return (new CommandResult(false, msg, null, HttpStatusCode.BadRequest), default);
            }

            var stock = JsonSerializer.Deserialize<Stock>(jSymbol.GetRawText(), new JsonSerializerOptions() { Converters = { new DateTimeConverterUsingDateTimeParse() } });

            return (new CommandResult(true, "Success", stock), stock);
        }

        private class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                System.Diagnostics.Debug.Assert(typeToConvert == typeof(DateTime));
                return DateTime.Parse(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

    }
}