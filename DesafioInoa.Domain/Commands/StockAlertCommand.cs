using Flunt.Validations;

namespace DesafioInoa.Domain.Commands
{
    public class StockAlertCommand : BaseCommand
    {
        public StockAlertCommand() { }

        public StockAlertCommand(string symbol, int sellValue, int buyValue)
        {
            Symbol = symbol;
            SellValue = sellValue;
            BuyValue = buyValue;
        }

        public string Symbol { get; set; }
        public int SellValue { get; set; }
        public int BuyValue { get; set; }

        public override void Validate()
        {
            AddNotifications(new Contract().Requires()
                .IsNotNullOrWhiteSpace(Symbol, "Symbol", "Symbol must not be null or empty")
                .IsGreaterThan(SellValue, 0, "SellValue", "SellValue must be greater than 0")
                .IsGreaterThan(BuyValue, 0, "BuyValue", "BuyValue must be greater than 0")
                .AreNotEquals(SellValue, BuyValue, "Value", "BuyValue cannot be equal to SellValue")
            );
        }
    }
}