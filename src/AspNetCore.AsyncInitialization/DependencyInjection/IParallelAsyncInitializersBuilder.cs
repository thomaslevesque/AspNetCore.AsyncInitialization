// ReSharper disable once CheckNamespace
using System;
using System.Threading.Tasks;
using AspNetCore.AsyncInitialization;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A builder to register async initializers that should run in parallel.
    /// </summary>
    public interface IParallelAsyncInitializersBuilder
    {
        /// <summary>
        /// Adds an async initializer of the specified type.
        /// </summary>
        /// <typeparam name="TInitializer">The type of the async initializer to add.</typeparam>
        /// <returns>The <see cref="IParallelAsyncInitializersBuilder" />.</returns>
        IParallelAsyncInitializersBuilder AddAsyncInitializer<TInitializer>()
            where TInitializer : class, IAsyncInitializer;

        /// <summary>
        /// Adds the specified async initializer instance.
        /// </summary>
        /// <typeparam name="TInitializer">The type of the async initializer to add.</typeparam>
        /// <param name="initializer">The service initializer</param>
        /// <returns>The <see cref="IParallelAsyncInitializersBuilder" />.</returns>
        IParallelAsyncInitializersBuilder AddAsyncInitializer<TInitializer>(TInitializer initializer)
            where TInitializer : class, IAsyncInitializer;

        /// <summary>
        /// Adds an async initializer with a factory specified in <paramref name="implementationFactory" />.
        /// </summary>
        /// <param name="implementationFactory">The factory that creates the async initializer.</param>
        /// <returns>The <see cref="IParallelAsyncInitializersBuilder" />.</returns>
        IParallelAsyncInitializersBuilder AddAsyncInitializer(Func<IServiceProvider, IAsyncInitializer> implementationFactory);

        /// <summary>
        /// Adds an async initializer of the specified type
        /// </summary>
        /// <param name="initializerType">The type of the async initializer to add.</param>
        /// <returns>The <see cref="IParallelAsyncInitializersBuilder" />.</returns>
        IParallelAsyncInitializersBuilder AddAsyncInitializer(Type initializerType);

        /// <summary>
        /// Adds an async initializer whose implementation is the specified delegate.
        /// </summary>
        /// <param name="initializer">The delegate that performs async initialization.</param>
        /// <returns>The <see cref="IParallelAsyncInitializersBuilder" />.</returns>
        IParallelAsyncInitializersBuilder AddAsyncInitializer(Func<Task> initializer);
    }
}
