using System;
using System.Threading.Tasks;
using AspNetCore.AsyncInitialization;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to register async initializers.
    /// </summary>
    public static class AsyncInitializationServiceCollectionExtensions
    {

        private static int _nextInitializerOrder = 0;

        /// <summary>
        /// Registers necessary services for async initialization support.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddAsyncInitialization(this IServiceCollection services)
        {
            services.TryAddTransient<RootInitializer>();
            return services;
        }

        /// <summary>
        /// Adds an async initializer of the specified type.
        /// </summary>
        /// <typeparam name="TInitializer">The type of the async initializer to add.</typeparam>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddAsyncInitializer<TInitializer>(this IServiceCollection services)
            where TInitializer : class, IAsyncInitializer
        {
            int order = _nextInitializerOrder++;
            return services
                .AddAsyncInitialization()
                .AddTransient<TInitializer>()
                .AddTransient<IOrderedAsyncInitializer>(sp => new OrderedAsyncInitializer(sp.GetRequiredService<TInitializer>(), order));
        }

        /// <summary>
        /// Adds the specified async initializer instance.
        /// </summary>
        /// <typeparam name="TInitializer">The type of the async initializer to add.</typeparam>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
        /// <param name="initializer">The service initializer</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddAsyncInitializer<TInitializer>(this IServiceCollection services, TInitializer initializer)
            where TInitializer : class, IAsyncInitializer
        {
            int order = _nextInitializerOrder++;
            return services
                .AddAsyncInitialization()
                .AddTransient<IOrderedAsyncInitializer>(sp => new OrderedAsyncInitializer(initializer, order));
        }

        /// <summary>
        /// Adds an async initializer with a factory specified in <paramref name="implementationFactory" />.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
        /// <param name="implementationFactory">The factory that creates the async initializer.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Func<IServiceProvider, IAsyncInitializer> implementationFactory)
        {
            int order = _nextInitializerOrder++;
            return services
                .AddAsyncInitialization()
                .AddTransient<IOrderedAsyncInitializer>(sp => new OrderedAsyncInitializer(implementationFactory(sp), order));
        }

        /// <summary>
        /// Adds an async initializer of the specified type
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
        /// <param name="initializerType">The type of the async initializer to add.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Type initializerType)
        {
            int order = _nextInitializerOrder++;
            return services
                .AddAsyncInitialization()
                .AddTransient(typeof(IAsyncInitializer), initializerType)
                .AddTransient<IOrderedAsyncInitializer>(sp => new OrderedAsyncInitializer((IAsyncInitializer) sp.GetRequiredService(initializerType), order));
        }

        /// <summary>
        /// Adds an async initializer whose implementation is the specified delegate.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
        /// <param name="initializer">The delegate that performs async initialization.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Func<Task> initializer)
        {
            int order = _nextInitializerOrder++;
            return services
                .AddAsyncInitialization()
                .AddTransient<IOrderedAsyncInitializer>(sp => new OrderedAsyncInitializer(new DelegateAsyncInitializer(initializer), order));
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
