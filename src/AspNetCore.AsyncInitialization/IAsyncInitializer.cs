using System.Threading.Tasks;

namespace AspNetCore.AsyncInitialization
{
    /// <summary>
    /// Represents a type that performs async initialization.
    /// </summary>
    public interface IAsyncInitializer
    {
        /// <summary>
        /// Performs async initialization.
        /// </summary>
        /// <returns>A task that represents the initialization completion.</returns>
        Task InitializeAsync();
    }
}
