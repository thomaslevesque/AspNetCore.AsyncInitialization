using System;
using System.Threading.Tasks;
using AspNetCore.AsyncInitialization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AsyncInitializationServiceCollectionExtensions
    {
        public static IServiceCollection AddAsyncInitializer<TInitializer>(this IServiceCollection services)
            where TInitializer : class, IAsyncInitializer
        {
            return services.AddTransient<IAsyncInitializer, TInitializer>();
        }

        public static IServiceCollection AddAsyncInitializer<TInitializer>(this IServiceCollection services, TInitializer initializer)
            where TInitializer : class, IAsyncInitializer
        {
            return services.AddSingleton<IAsyncInitializer>(initializer);
        }

        public static IServiceCollection AddAsyncInitializer(this IServiceCollection services, Func<Task> initializer)
        {
            return services.AddSingleton<IAsyncInitializer>(new DelegateAsyncInitializer(initializer));
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
