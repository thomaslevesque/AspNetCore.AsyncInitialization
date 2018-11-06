namespace AspNetCore.AsyncInitialization
{
    internal interface IOrderedAsyncInitializer
    {
        IAsyncInitializer Initializer { get; }
        int Order { get; }
    }
}