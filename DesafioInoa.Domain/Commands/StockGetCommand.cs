using Flunt.Validations;

namespace DesafioInoa.Domain.Commands
{
    public class StockGetCommand : BaseCommand
    {
        public StockGetCommand() { }

        public string Symbol { get; set; }

        public override void Validate()
        {
            AddNotifications(new Contract().Requires()
                .IsNotNullOrWhiteSpace(Symbol, "Symbol", "Symbol vazio ou inválido")
            );
        }
    }
}