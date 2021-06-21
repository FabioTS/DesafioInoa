using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DesafioInoa.Api.Services;
using DesafioInoa.App;
using DesafioInoa.Domain.Commands;
using DesafioInoa.Domain.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DesafioInoa.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockController : ControllerBase
    {
        private readonly ILogger<StockController> _logger;
        private readonly TokenStorageService _tokenStorageService;

        public StockController(ILogger<StockController> logger, TokenStorageService tokenStorageService)
        {
            _logger = logger ?? throw new ArgumentNullException("ILogger");
            _tokenStorageService = tokenStorageService ?? throw new ArgumentNullException("TokenStorageService");
        }

        /// <summary>
        /// Get Stock information
        /// </summary>
        /// <remarks>
        /// Obtem informaçoes relevantes sobre uma ação da B3.
        /// </remarks>
        /// <param name="symbol"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ICommandResult> GetStock(
            [FromQuery] string symbol,
            [FromServices] StockHandler handler
        )
        {
            var commandResult = await handler.Handle(new StockGetCommand(symbol));
            this.HttpContext.Response.StatusCode = (int)commandResult.HttpStatusCode;
            return commandResult;
        }

        /// <summary>
        /// Stock Alert email
        /// </summary>
        /// <remarks>
        /// Envia um email caso uma ação esteja acima ou abaixa do preço recomendado.
        /// </remarks>
        /// <param name="command"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ICommandResult> PostStockAlert(
            [FromBody] StockAlertCommand command,
            [FromServices] StockHandler handler
        )
        {
            var commandResult = await handler.Handle(command);
            this.HttpContext.Response.StatusCode = (int)commandResult.HttpStatusCode;
            return commandResult;
        }

        /// <summary>
        /// Stock alert start monitor
        /// </summary>
        /// <remarks>
        /// Instancia um novo processo que irá monitorar os preços de uma ação e, 
        /// Envia um email caso uma ação esteja acima ou abaixa do preço recomendado.
        /// </remarks>
        /// <param name="command"></param>
        /// <param name="svc"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("monitor")]
        public ICommandResult PostStockAlertMonitor(
            [FromBody] StockAlertCommand command,
            [FromServices] StockQuoteAlert svc
        )
        {
            if (!command.IsValid())
            {
                this.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new CommandResult(false, "Invalid command", command.Notifications, HttpStatusCode.BadRequest);
            }

            var args = new string[] {
                AppContext.BaseDirectory,
                command.Symbol,
                command.SellValue.ToString(),
                command.BuyValue.ToString()
            };
            Environment.SetEnvironmentVariable(StockQuoteAlert.ARGS_ENV_VAR, string.Join(' ', args));
            var cancellationTokenSource = new CancellationTokenSource();
            var task = Task.Run(() => svc.StartAsync(cancellationTokenSource.Token));
            _tokenStorageService.CancellationTokens.Add(task.Id, cancellationTokenSource);

            return new CommandResult(true, "OK", new { task.Id });
        }

        /// <summary>
        /// Stock alert stop monitor
        /// </summary>
        /// <remarks>
        /// Termina o processo de monitoramento especificado por "id".
        /// Caso não seja especificado nenhum (0), todos os processos de monitoramento em execução serão finalizados.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("monitor")]
        public ICommandResult DeleteStockAlertMonitor(
            [FromQuery] int id
        )
        {
            if (id == default)
            {
                foreach (var item in _tokenStorageService.CancellationTokens)
                {
                    item.Value.Cancel();
                    item.Value.Dispose();
                }
                _tokenStorageService.CancellationTokens.Clear();
                return new CommandResult(true, "All monitors stopped");
            }

            if (!_tokenStorageService.CancellationTokens.ContainsKey(id))
                return new CommandResult(false, "Could not find task with ID", new { id }, HttpStatusCode.BadRequest);

            _tokenStorageService.CancellationTokens[id].Cancel();
            _tokenStorageService.CancellationTokens[id].Dispose();
            _tokenStorageService.CancellationTokens.Remove(id);

            return new CommandResult(true, "Monitor stopped", new { id });
        }
    }
}
