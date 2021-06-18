namespace DesafioInoa.Domain.Commands
{
    public interface ICommandResult { }
    public class CommandResult : ICommandResult
    {
        public CommandResult() { }
        public CommandResult(bool success, string message, dynamic data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public bool Success { get; }
        public string Message { get; }
        public object Data { get; }

    }
}