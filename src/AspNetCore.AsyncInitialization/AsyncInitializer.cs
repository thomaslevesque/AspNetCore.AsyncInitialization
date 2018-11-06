using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AspNetCore.AsyncInitialization
{
    internal class RootInitializer
    {
        private readonly ILogger<RootInitializer> _logger;
        private readonly IEnumerable<IOrderedAsyncInitializer> _initializers;

        public RootInitializer(ILogger<RootInitializer> logger, IEnumerable<IOrderedAsyncInitializer> initializers)
        {
            _logger = logger;
            _initializers = initializers;
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Starting async initialization");

            try
            {
                var initializers = _initializers
                    .OrderBy(i => i.Order)
                    .GroupBy(i => i.Order, i => i.Initializer);

                foreach (var group in initializers)
                {
                    var tasks = group.Select(async initializer =>
                    {
                        _logger.LogInformation("Starting async initialization for {InitializerType}", initializer.GetType());
                        await initializer.InitializeAsync();
                        _logger.LogInformation("Async initialization for {InitializerType} completed", initializer.GetType());
                    }).ToArray();

                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch
                    {
                        // Ignore exception here, as we don't know which task caused it
                    }

                    var initializersWithTasks = group.Zip(tasks, (i, t) => new { Initializer = i, Task = t });
                    foreach (var i in initializersWithTasks)
                    {
                        if (i.Task.Status != TaskStatus.RanToCompletion)
                        {
                            try
                            {
                                await i.Task;
                            }
                            catch(Exception ex)
                            {
                                _logger.LogError(ex, "Async initialization for {InitializerType} failed", i.Initializer.GetType());
                                throw;
                            }
                        }
                    }
                }

                _logger.LogInformation("Async initialization completed");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Async initialization failed");
                throw;
            }
        }
    }
}