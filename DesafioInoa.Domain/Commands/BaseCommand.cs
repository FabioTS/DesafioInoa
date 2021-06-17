using Flunt.Notifications;

namespace DesafioInoa.Domain.Commands
{
    public abstract class BaseCommand : Notifiable, ICommand
    {
        public bool IsValid()
        {
            Validate();
            return Valid;
        }

        public virtual void Validate()
        {
        }
    }
}