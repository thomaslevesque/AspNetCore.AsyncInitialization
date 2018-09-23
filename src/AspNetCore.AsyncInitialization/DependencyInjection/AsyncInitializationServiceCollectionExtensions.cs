using System;
using System.Threading.Tasks;
using AspNetCore.AsyncInitialization;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class AsyncInitializationServiceCollectionExtensions
    {
        public static IServiceCollection AddAsyncInitialization(this IServiceCollection services)
        {
            services.TryAddTransient<AsyncInitializer>();
            return services;
        }

        public static IServiceCollection AddAsyncInitializer<TInitializer>(this IServiceCollection services)
            where TInitializer : class, IAsyncInitializer
        {
            return services
                .AddAsyncInitialization()
                .AddTransient<IAsyncInitializer, TInitializer>();
        }

        public static IServiceCollection AddAsyncInitializer<TInitializer>(this IServiceCollection services, TInitializer initializer)
            where TInitializer : class, IAsyncInitializer
        {
            return services
                .AddAsyncInitialization()
                .AddSingleton<IAsyncInitializer>(initializer);
        }

        public static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Func<IServiceProvider, IAsyncInitializer> implementationFactory)
        {
            return services
                .AddAsyncInitialization()
                .AddTransient(implementationFactory);
        }

        public static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Func<Task> initializer)
        {
            return services
                .AddAsyncInitialization()
                .AddSingleton<IAsyncInitializer>(new DelegateAsyncInitializer(initializer));
        }

        private class DelegateAsyncInitializer : IAsyncInitializer
        {
            private readonly Func<Task> _initializer;

            public DelegateAsyncInitializer(Func<Task> initializer)
            {
                _initializer = initializer;
            }

            public Task InitializeAsync()
            {
                return _initializer();
            }
        }
    }
}
