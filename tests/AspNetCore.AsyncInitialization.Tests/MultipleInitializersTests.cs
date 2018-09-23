using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspNetCore.AsyncInitialization.Tests
{
    public class MultipleInitializersTests
    {
        [Fact]
        public async Task InitializersAreCalled()
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
                services.AddAsyncInitializer<Initializer1>();
                services.AddAsyncInitializer<Initializer2>();
            }

            public void Configure(IApplicationBuilder app)
            {
            }
        }

        public class Initializer1 : IAsyncInitializer
        {
            private readonly IService _service;

            public Initializer1(IService service)
            {
                _service = service;
            }

            public async Task InitializeAsync()
            {
                await Task.Delay(100);
                _service.SetInitialized();
            }
        }

        public class Initializer2 : IAsyncInitializer
        {
            private readonly IService _service;

            public Initializer2(IService service)
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
            private int _count;

            public void SetInitialized()
            {
                _count++;
            }

            public bool IsInitialized => _count == 2;
        }

    }
}
