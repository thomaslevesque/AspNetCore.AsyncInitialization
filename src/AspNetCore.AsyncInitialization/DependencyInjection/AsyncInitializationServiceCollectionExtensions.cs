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
            return services.AddAsyncInitializer<TInitializer>(GetNextOrder());
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
            return services.AddAsyncInitializer<TInitializer>(initializer, GetNextOrder());
        }

        /// <summary>
        /// Adds an async initializer with a factory specified in <paramref name="implementationFactory" />.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
        /// <param name="implementationFactory">The factory that creates the async initializer.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Func<IServiceProvider, IAsyncInitializer> implementationFactory)
        {
            return services.AddAsyncInitializer(implementationFactory, GetNextOrder());
        }

        /// <summary>
        /// Adds an async initializer of the specified type
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
        /// <param name="initializerType">The type of the async initializer to add.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Type initializerType)
        {
            return services.AddAsyncInitializer(initializerType, GetNextOrder());
        }

        /// <summary>
        /// Adds an async initializer whose implementation is the specified delegate.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
        /// <param name="initializer">The delegate that performs async initialization.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Func<Task> initializer)
        {
            return services.AddAsyncInitializer(initializer, GetNextOrder());
        }

        /// <summary>
        /// Adds multiple async initializers that should run in parallel.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the services to.</param>
        /// <param name="build">The action that adds parallel initializers.</param>
        public static IServiceCollection AddParallelAsyncInitializers(this IServiceCollection services, Action<IParallelAsyncInitializersBuilder> build)
        {
            int order = GetNextOrder();
            services.AddAsyncInitialization();
            var builder = new ParallelAsyncInitializersBuilder(services, order);
            build(builder);
            return services;
        }

        private static int GetNextOrder() => _nextInitializerOrder++;

        private static IServiceCollection AddAsyncInitializer<TInitializer>(this IServiceCollection services, int order)
            where TInitializer : class, IAsyncInitializer
        {
            return services
                .AddTransient<TInitializer>()
                .AddAsyncInitializerCore(sp => sp.GetRequiredService<TInitializer>(), order);
        }

        private static IServiceCollection AddAsyncInitializer<TInitializer>(this IServiceCollection services, TInitializer initializer, int order)
            where TInitializer : class, IAsyncInitializer
        {
            return services.AddAsyncInitializerCore(sp => initializer, order);
        }

        private static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Func<IServiceProvider, IAsyncInitializer> implementationFactory, int order)
        {
            return services.AddAsyncInitializerCore(implementationFactory, order);
        }

        private static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Type initializerType, int order)
        {
            return services
                .AddTransient(initializerType)
                .AddAsyncInitializerCore(sp => (IAsyncInitializer) sp.GetRequiredService(initializerType), order);
        }

        private static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Func<Task> initializer, int order)
        {
            return services.AddAsyncInitializerCore(sp => new DelegateAsyncInitializer(initializer), order);
        }

        private static IServiceCollection AddAsyncInitializerCore(this IServiceCollection services, Func<IServiceProvider, IAsyncInitializer> factory)
        {
            return services.AddAsyncInitializerCore(factory, GetNextOrder());
        }

        private static IServiceCollection AddAsyncInitializerCore(this IServiceCollection services, Func<IServiceProvider, IAsyncInitializer> factory, int order)
        {
            return services
                .AddAsyncInitialization()
                .AddTransient<IOrderedAsyncInitializer>(sp => new OrderedAsyncInitializer(factory(sp), order));
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

        private class ParallelAsyncInitializersBuilder : IParallelAsyncInitializersBuilder
        {
            private readonly IServiceCollection _services;
            private readonly int _order;

            public ParallelAsyncInitializersBuilder(IServiceCollection services, int order)
            {
                _services = services;
                _order = order;
            }

            public IParallelAsyncInitializersBuilder AddAsyncInitializer<TInitializer>()
                where TInitializer : class, IAsyncInitializer
            {
                _services.AddAsyncInitializer<TInitializer>(_order);
                return this;
            }

            public IParallelAsyncInitializersBuilder AddAsyncInitializer<TInitializer>(TInitializer initializer)
                where TInitializer : class, IAsyncInitializer
            {
                _services.AddAsyncInitializer(initializer, _order);
                return this;
            }

            public IParallelAsyncInitializersBuilder AddAsyncInitializer(Func<IServiceProvider, IAsyncInitializer> implementationFactory)
            {
                _services.AddAsyncInitializer(implementationFactory, _order);
                return this;
            }

            public IParallelAsyncInitializersBuilder AddAsyncInitializer(Type initializerType)
            {
                _services.AddAsyncInitializer(initializerType, _order);
                return this;
            }

            public IParallelAsyncInitializersBuilder AddAsyncInitializer(Func<Task> initializer)
            {
                _services.AddAsyncInitializer(initializer, _order);
                return this;
            }
        }
    }
}
