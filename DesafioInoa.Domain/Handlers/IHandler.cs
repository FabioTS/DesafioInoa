using System.Threading.Tasks;
using DesafioInoa.Domain.Commands;

namespace DesafioInoa.Domain.Handlers
{
    public interface IHandler<T> where T : ICommand
    {
        Task<CommandResult> Handle(T command);
    }
}