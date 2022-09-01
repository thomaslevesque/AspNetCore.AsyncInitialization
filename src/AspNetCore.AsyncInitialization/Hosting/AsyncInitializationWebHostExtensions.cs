using System;
using System.Threading.Tasks;
using AspNetCore.AsyncInitialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Provides extension methods to perform async initialization of an application.
    /// </summary>
    public static class AsyncInitializationWebHostExtensions
    {
        /// <summary>
        /// Initializes the application, by calling all registered async initializers.
        /// </summary>
        /// <param name="host">The web host.</param>
        /// <returns>A task that represents the initialization completion.</returns>
        public static async Task InitAsync(this IWebHost host)
        {
            await Init(host.Services);
        }
        
        /// <summary>
        /// Initializes the application, by calling all registered async initializers.
        /// </summary>
        /// <param name="host">The web host.</param>
        /// <returns>A task that represents the initialization completion.</returns>
        public static async Task InitAsync(this IHost host)
        {
            await Init(host.Services);
        }

        private static async Task Init(IServiceProvider sp)
        {
            using (var scope = sp.CreateScope())
            {
                var rootInitializer = scope.ServiceProvider.GetService<RootInitializer>();
                if (rootInitializer == null)
                {
                    throw new InvalidOperationException(
                        "The async initialization service isn't registered, register it by calling AddAsyncInitialization() on the service collection or by adding an async initializer.");
                }

                await rootInitializer.InitializeAsync();
            }
        }

        
    }
}