using Flunt.Validations;

namespace DesafioInoa.Domain.Commands
{
    public interface ICommand : IValidatable
    {
        public bool IsValid();
    }
}