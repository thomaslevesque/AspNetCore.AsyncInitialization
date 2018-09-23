using System.Threading.Tasks;
using AspNetCore.AsyncInitialization;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{
    public static class AsyncInitializationWebHostExtensions
    {
        public static async Task InitAsync(this IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<AsyncInitializer>();
                await initializer.InitializeAsync();
            }
        }
    }
}
