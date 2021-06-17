using System.Threading.Tasks;
using DesafioInoa.Domain.Commands;

namespace DesafioInoa.Domain.Services
{
    public interface IMailService
    {
        Task<CommandResult> SendMail(string to, string subject, string body);
    }
}