using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AspNetCore.AsyncInitialization
{
    internal class AsyncInitializer
    {
        private readonly ILogger<AsyncInitializer> _logger;
        private readonly IEnumerable<IAsyncInitializer> _initializers;

        public AsyncInitializer(ILogger<AsyncInitializer> logger, IEnumerable<IAsyncInitializer> initializers)
        {
            _logger = logger;
            _initializers = initializers;
        }

        public async Task InitializeAsync()
        {
            _logger.LogDebug("Starting async initialization");

            try
            {
                foreach (var initializer in _initializers)
                {
                    _logger.LogDebug("Starting async initialization for {InitializerType}", initializer.GetType());
                    try
                    {
                        await initializer.InitializeAsync();
                        _logger.LogDebug("Async initialization for {InitializerType} completed", initializer.GetType());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Async initialization for {InitializerType} failed", initializer.GetType());
                        throw;
                    }
                }

                _logger.LogDebug("Async initialization completed");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Async initialization failed");
            }
        }
    }
}