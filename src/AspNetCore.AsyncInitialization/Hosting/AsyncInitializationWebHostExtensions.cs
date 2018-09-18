using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetCore.AsyncInitialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Hosting
{
    public static class AsyncInitializationWebHostExtensions
    {
        public static IWebHostBuilder UseAsyncInitialization(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddTransient<AsyncInitializer>();
            });
        }

        public static async Task InitAsync(this IWebHost host)
        {
            var initializer = host.Services.GetRequiredService<AsyncInitializer>();
            await initializer.InitializeAsync();
        }

        private class AsyncInitializer
        {
            private readonly ILogger<AsyncInitializer> _logger;
            private readonly IEnumerable<IAsyncInitializer> _initializers;

            public AsyncInitializer(ILogger<AsyncInitializer> logger, IEnumerable<IAsyncInitializer> initializers)
            {
                this._logger = logger;
                this._initializers = initializers;
            }

            public async Task InitializeAsync()
            {
                _logger.LogDebug("Starting async initialization");

                try
                {
                    foreach (var initializer in _initializers)
                    {
                        var scopeData = new Dictionary<string, object>{ { "InitializerType", initializer.GetType() } };
                        using (_logger.BeginScope(scopeData))
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
}
