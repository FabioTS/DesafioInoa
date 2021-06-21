using System;
using System.Threading.Tasks;
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

        public StockController(ILogger<StockController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException("ILogger");
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
            CommandResult commandResult = await handler.Handle(new StockGetCommand(symbol));

            this.HttpContext.Response.StatusCode = (int)commandResult.HttpStatusCode;
            return commandResult;
        }
    }
}
