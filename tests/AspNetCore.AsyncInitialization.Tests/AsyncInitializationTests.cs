using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspNetCore.AsyncInitialization.Tests
{
    public class AsyncInitializationTests
    {
        [Fact]
        public async Task Single_initializer_is_called()
        {
            var initializer = A.Fake<IAsyncInitializer>();

            var host = CreateHost(services => services.AddAsyncInitializer(initializer));

            await host.InitAsync();

            A.CallTo(() => initializer.InitializeAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Delegate_initializer_is_called()
        {
            var initializer = A.Fake<Func<Task>>();

            var host = CreateHost(services => services.AddAsyncInitializer(initializer));

            await host.InitAsync();

            A.CallTo(() => initializer()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Multiple_initializers_are_called_in_order()
        {
            var initializer1 = A.Fake<IAsyncInitializer>();
            var initializer2 = A.Fake<IAsyncInitializer>();
            var initializer3 = A.Fake<IAsyncInitializer>();

            var host = CreateHost(services =>
            {
                services.AddAsyncInitializer(initializer1);
                services.AddAsyncInitializer(initializer2);
                services.AddAsyncInitializer(initializer3);
            });

            await host.InitAsync();

            A.CallTo(() => initializer1.InitializeAsync()).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => initializer2.InitializeAsync()).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => initializer3.InitializeAsync()).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public async Task Initializer_with_scoped_dependency_is_resolved()
        {
            var host = CreateHost(
                services =>
                {
                    services.AddScoped(sp => A.Fake<IDependency>());
                    services.AddAsyncInitializer<Initializer>();
                },
                true);

            await host.InitAsync();
        }

        [Fact]
        public async Task Failing_initializer_makes_initialization_fail()
        {
            var initializer1 = A.Fake<IAsyncInitializer>();
            var initializer2 = A.Fake<IAsyncInitializer>();
            var initializer3 = A.Fake<IAsyncInitializer>();

            A.CallTo(() => initializer2.InitializeAsync()).ThrowsAsync(() => new Exception("oops"));

            var host = CreateHost(services =>
            {
                services.AddAsyncInitializer(initializer1);
                services.AddAsyncInitializer(initializer2);
                services.AddAsyncInitializer(initializer3);
            });

            var exception = await Record.ExceptionAsync(() => host.InitAsync());
            Assert.IsType<Exception>(exception);
            Assert.Equal("oops", exception.Message);

            A.CallTo(() => initializer1.InitializeAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => initializer3.InitializeAsync()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Parallel_initializers_are_run_in_parallel()
        {
            var initializers = Enumerable.Range(0, 6)
                .Select(_ => CreateInitializer())
                .ToList();

            var host = CreateHost(services =>
            {
                // 0 and 1 in parallel
                // then 2 alone
                // then 3, 4 and 5 in parallel

                services.AddParallelAsyncInitializers(builder => 
                {
                    builder.AddAsyncInitializer(initializers[0].initializer);
                    builder.AddAsyncInitializer(initializers[1].initializer);
                });
                services.AddAsyncInitializer(initializers[2].initializer);
                services.AddParallelAsyncInitializers(builder => 
                {
                    builder.AddAsyncInitializer(initializers[3].initializer);
                    builder.AddAsyncInitializer(initializers[4].initializer);
                    builder.AddAsyncInitializer(initializers[5].initializer);
                });
            });

            var initializationTask = host.InitAsync();
            MustHaveStarted(0, 1);
            MustNotHaveStarted(2, 3, 4, 5);
            
            CompleteInitializers(0, 1);
            MustHaveStarted(2);
            MustNotHaveStarted(3, 4, 5);

            CompleteInitializers(2);
            MustHaveStarted(3, 4, 5);
            CompleteInitializers(3, 4, 5);

            await initializationTask;

            void MustHaveStarted(params int[] initializerIndexes)
            {
                foreach (var index in initializerIndexes)
                {
                    A.CallTo(() => initializers[index].initializer.InitializeAsync()).MustHaveHappened();
                }
            }

            void MustNotHaveStarted(params int[] initializerIndexes)
            {
                foreach (var index in initializerIndexes)
                {
                    A.CallTo(() => initializers[index].initializer.InitializeAsync()).MustNotHaveHappened();
                }
            }

            void CompleteInitializers(params int[] initializerIndexes)
            {
                foreach (var index in initializerIndexes)
                {
                    initializers[index].completionSource.SetResult(0);
                }
            }

            (IAsyncInitializer initializer, TaskCompletionSource<int> completionSource) CreateInitializer()
            {
                var initializer = A.Fake<IAsyncInitializer>();
                var tcs = new TaskCompletionSource<int>();
                A.CallTo(() => initializer.InitializeAsync()).Returns(tcs.Task);
                return (initializer, tcs);
            }
        }

        private static IWebHost CreateHost(Action<IServiceCollection> configureServices, bool validateScopes = false) =>
            new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(configureServices)
                .UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes)
                .Build();

        public class Startup
        {
            public void Configure(IApplicationBuilder app)
            {
            }
        }

        public interface IDependency
        {
        }

        public class Initializer : IAsyncInitializer
        {
            // ReSharper disable once NotAccessedField.Local
            private readonly IDependency _dependency;

            public Initializer(IDependency dependency)
            {
                _dependency = dependency;
            }
            public Task InitializeAsync() => Task.CompletedTask;
        }
    }
}