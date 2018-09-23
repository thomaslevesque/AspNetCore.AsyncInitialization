using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspNetCore.AsyncInitialization.Tests
{
    public class SingleInitializerTests
    {
        [Fact]
        public async Task InitializerIsCalled()
        {
            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseAsyncInitialization()
                .Build();

            await host.InitAsync();

            var service = host.Services.GetRequiredService<IService>();
            Assert.True(service.IsInitialized);
        }

        public class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<IService, Service>();
                services.AddAsyncInitializer<Initializer>();
            }

            public void Configure(IApplicationBuilder app)
            {
            }
        }

        public class Initializer : IAsyncInitializer
        {
            private readonly IService _service;

            public Initializer(IService service)
            {
                _service = service;
            }

            public async Task InitializeAsync()
            {
                await Task.Delay(100);
                _service.SetInitialized();
            }
        }

        public interface IService
        {
            void SetInitialized();
            bool IsInitialized { get; }
        }

        public class Service : IService
        {
            public void SetInitialized()
            {
                IsInitialized = true;
            }

            public bool IsInitialized { get; private set; }
        }
    }
}
