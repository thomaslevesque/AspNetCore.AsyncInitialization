using System.Threading.Tasks;

namespace AspNetCore.AsyncInitialization
{
    public interface IAsyncInitializer
    {
        Task InitializeAsync();
    }
}
