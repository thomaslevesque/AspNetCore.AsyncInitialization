using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspNetCore.AsyncInitialization.Tests
{
    public class DelegateInitializerTests
    {
        [Fact]
        public async Task InitializerIsCalled()
        {
            Startup.IsInitialized = false;

            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseAsyncInitialization()
                .Build();

            await host.InitAsync();

            Assert.True(Startup.IsInitialized);
        }

        public class Startup
        {
            public static bool IsInitialized { get; set; }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddAsyncInitializer(async () =>
                {
                    await Task.Delay(100);
                    IsInitialized = true;
                });
            }

            public void Configure(IApplicationBuilder app)
            {
            }
        }
    }
}
