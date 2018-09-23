using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspNetCore.AsyncInitialization.Tests
{
    public class InitializerWithScopedDependencyTests
    {
        [Fact]
        public async Task InitializerIsCalled()
        {
            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseDefaultServiceProvider(options => options.ValidateScopes = true)
                .Build();

            await host.InitAsync();

            var service = host.Services.GetRequiredService<IService>();
            Assert.True(service.IsInitialized);
        }

        public class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddScoped<IScopedService, ScopedService>();
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
            // ReSharper disable once NotAccessedField.Local
            private readonly IScopedService _scopedService;

            public Initializer(IService service, IScopedService scopedService)
            {
                _service = service;
                _scopedService = scopedService;
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

        public interface IScopedService { }
        public class ScopedService : IScopedService { }
    }
}
