using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace DesafioInoa.Api.Services
{
    public class TokenStorageService
    {
        private readonly ILogger _logger;
        public IDictionary<int, CancellationTokenSource> CancellationTokens;

        public TokenStorageService(ILogger<TokenStorageService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException("ILogger");
            CancellationTokens = new Dictionary<int, CancellationTokenSource>();
        }

    }
}