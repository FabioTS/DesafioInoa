using System.Net;

namespace DesafioInoa.Domain.Commands
{
    public interface ICommandResult { }
    public class CommandResult : ICommandResult
    {
        public CommandResult() { }
        public CommandResult(bool success, string message, dynamic data = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            Success = success;
            Message = message;
            Data = data;
            HttpStatusCode = httpStatusCode;
        }

        public HttpStatusCode HttpStatusCode { get; }
        public bool Success { get; }
        public string Message { get; }
        public object Data { get; }

    }
}